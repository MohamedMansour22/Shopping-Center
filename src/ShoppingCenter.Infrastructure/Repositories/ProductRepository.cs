using Microsoft.EntityFrameworkCore;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;
using ShoppingCenter.Infrastructure.Data;

namespace ShoppingCenter.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(bool includeHidden, CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsNoTracking();
        if (!includeHidden)
        {
            query = query.Where(p => !p.IsHidden);
        }

        var products = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        await AttachPrimaryImagesAsync(products, cancellationToken);
        return products;
    }

    // List responses only need the primary image, so we load just SortOrder 0 for the page's products
    // (one extra query) instead of Including every image — keeps the payload lean under pagination.
    private async Task AttachPrimaryImagesAsync(IReadOnlyList<Product> products, CancellationToken cancellationToken)
    {
        if (products.Count == 0)
            return;

        var ids = products.Select(p => p.Id).ToList();
        var primaries = await _db.ProductImages
            .AsNoTracking()
            .Where(img => ids.Contains(img.ProductId) && img.SortOrder == 0)
            .ToListAsync(cancellationToken);

        var byProduct = primaries.ToDictionary(img => img.ProductId);
        foreach (var p in products)
        {
            p.Images = byProduct.TryGetValue(p.Id, out var img)
                ? new List<ProductImage> { img }
                : new List<ProductImage>();
        }
    }

    // Storefront paged read: always visible-only, with a single name-OR-category search term.
    // excludeSoldOut is a mechanism the service drives from its browse-vs-search policy.
    public Task<(IReadOnlyList<Product> Items, int TotalCount)> GetStorefrontPageAsync(
        int page, int pageSize, string? search, bool excludeSoldOut,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsNoTracking().Where(p => !p.IsHidden);

        if (excludeSoldOut)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // Case-insensitive by default under SQL Server's standard collation; matches name OR category.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = LikePattern.Contains(search.Trim());
            query = query.Where(p =>
                EF.Functions.Like(p.Name, pattern) ||
                EF.Functions.Like(p.Category, pattern));
        }

        return PageAsync(query, page, pageSize, cancellationToken);
    }

    // Admin paged read: choose which products to include by visibility, and optionally narrow by
    // name and/or category (each ANDed). Sold-out products stay visible for stock management.
    public Task<(IReadOnlyList<Product> Items, int TotalCount)> GetAdminPageAsync(
        int page, int pageSize, ProductVisibilityFilter visibility, string? name, string? category,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsNoTracking();
        query = visibility switch
        {
            ProductVisibilityFilter.VisibleOnly => query.Where(p => !p.IsHidden),
            ProductVisibilityFilter.HiddenOnly => query.Where(p => p.IsHidden),
            _ => query, // All
        };

        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = LikePattern.Contains(name.Trim());
            query = query.Where(p => EF.Functions.Like(p.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var pattern = LikePattern.Contains(category.Trim());
            query = query.Where(p => EF.Functions.Like(p.Category, pattern));
        }

        return PageAsync(query, page, pageSize, cancellationToken);
    }

    // Shared paging: count and slice come off the SAME filtered query, so HasMore stays correct.
    private async Task<(IReadOnlyList<Product> Items, int TotalCount)> PageAsync(
        IQueryable<Product> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        // CreatedAtUtc alone is not a total order (the seeder can share timestamps across rows),
        // so ThenBy(Id) gives a stable order — without it Skip/Take could duplicate or drop products.
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        await AttachPrimaryImagesAsync(items, cancellationToken);
        return (items, totalCount);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Single product: load all its images (the detail gallery and admin editor need every image).
        return await _db.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        // Tracked (no AsNoTracking) and without images: the caller mutates stock and relies on the
        // shared DbContext's next SaveChanges to persist those changes alongside the new order.
        return await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> UpdateAsync(
        Product product,
        IReadOnlyList<Guid> keepImageIds,
        IReadOnlyList<ProductImage> newImages,
        CancellationToken cancellationToken = default)
    {
        // Reconciling a child collection requires a tracked load (EF won't delete removed children
        // off a detached graph), so we re-read the product with its images and mutate in place.
        var existing = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);
        if (existing is null)
            return null;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.Category = product.Category;

        // Delete images the client didn't ask to keep.
        var keep = keepImageIds.ToHashSet();
        var toRemove = existing.Images.Where(img => !keep.Contains(img.Id)).ToList();
        _db.ProductImages.RemoveRange(toRemove);

        // Re-number kept images by their position in keepImageIds (this defines the new order/primary).
        var order = 0;
        foreach (var keepId in keepImageIds)
        {
            var img = existing.Images.FirstOrDefault(i => i.Id == keepId);
            if (img is not null)
                img.SortOrder = order++;
        }

        // Append the newly uploaded images after the kept ones. Add them to the DbSet explicitly
        // (rather than via the navigation collection): the images already carry a non-default Id from
        // their initializer, so EF's add-vs-modify key heuristic would otherwise mis-mark them as
        // existing rows and emit an UPDATE that affects 0 rows. Add() forces the INSERT.
        foreach (var img in newImages)
        {
            img.SortOrder = order++;
            img.ProductId = existing.Id;
            _db.ProductImages.Add(img);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
            return false;

        // Image rows are removed by the ON DELETE CASCADE configured on the FK.
        _db.Products.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetVisibilityAsync(Guid id, bool isHidden, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
            return false;

        product.IsHidden = isHidden;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
