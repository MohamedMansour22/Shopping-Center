using Microsoft.EntityFrameworkCore;
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

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, bool includeHidden, CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsNoTracking();
        if (!includeHidden)
        {
            query = query.Where(p => !p.IsHidden);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            // Case-insensitive by default under SQL Server's standard collation; matches name or category.
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%") ||
                EF.Functions.Like(p.Category, $"%{term}%"));
        }

        // Count and slice come off the SAME filtered query, so HasMore stays correct under search.
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
