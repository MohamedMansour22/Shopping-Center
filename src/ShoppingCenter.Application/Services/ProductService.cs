using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;
using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category?.Trim() ?? string.Empty,
            Images = dto.Images
                .Select((img, i) => new ProductImage
                {
                    ImageData = img.Data,
                    ImageContentType = img.ContentType,
                    SortOrder = i
                })
                .ToList()
        };

        var saved = await _repository.AddAsync(product, cancellationToken);
        return ToDto(saved);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeHidden, CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(includeHidden, cancellationToken);
        return products.Select(ToDto).ToList();
    }

    public async Task<PagedResult<ProductDto>> GetStorefrontPageAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        // Merchandising policy: hide sold-out products when the customer is browsing, but keep them
        // discoverable when they're actively searching for something specific.
        var excludeSoldOut = string.IsNullOrWhiteSpace(search);
        var (items, totalCount) = await _repository.GetStorefrontPageAsync(
            page, pageSize, search, excludeSoldOut, cancellationToken);
        return ToPagedResult(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<ProductDto>> GetAdminPageAsync(
        int page, int pageSize, ProductVisibilityFilter visibility, string? name, string? category,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetAdminPageAsync(
            page, pageSize, visibility, name, category, cancellationToken);
        return ToPagedResult(items, totalCount, page, pageSize);
    }

    private static PagedResult<ProductDto> ToPagedResult(
        IReadOnlyList<Product> items, int totalCount, int page, int pageSize) =>
        new()
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            HasMore = (long)page * pageSize < totalCount
        };

    public async Task<ProductDto?> GetByIdAsync(Guid id, bool includeHidden, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null || (!includeHidden && product.IsHidden))
            return null;
        return ToDto(product);
    }

    public Task<bool> SetVisibilityAsync(Guid id, bool isHidden, CancellationToken cancellationToken = default)
        => _repository.SetVisibilityAsync(id, isHidden, cancellationToken);

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return null;

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.Category = dto.Category?.Trim() ?? string.Empty;

        var newImages = dto.NewImages
            .Select(img => new ProductImage
            {
                ImageData = img.Data,
                ImageContentType = img.ContentType
            })
            .ToList();

        // The repository reconciles the image collection (keep / delete / append) against the DB.
        var updated = await _repository.UpdateAsync(product, dto.KeepImageIds, newImages, cancellationToken);
        return updated is null ? null : ToDto(updated);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    private static ProductDto ToDto(Product p)
    {
        // Include order is not guaranteed, so order by SortOrder here (primary first).
        var images = p.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageDataUri = $"data:{i.ImageContentType};base64,{Convert.ToBase64String(i.ImageData)}"
            })
            .ToList();

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Category = p.Category,
            HasImage = images.Count > 0,
            ImageDataUri = images.FirstOrDefault()?.ImageDataUri,
            Images = images,
            IsHidden = p.IsHidden,
            CreatedAtUtc = p.CreatedAtUtc
        };
    }
}
