using ShoppingCenter.Application.DTOs;

namespace ShoppingCenter.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    // includeHidden is true only for admin callers; customers never see hidden products.
    Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeHidden, CancellationToken cancellationToken = default);

    // Server-side paged read with an optional name/category search term.
    Task<PagedResult<ProductDto>> GetPagedAsync(
        int page, int pageSize, string? search, bool includeHidden, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, bool includeHidden, CancellationToken cancellationToken = default);

    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetVisibilityAsync(Guid id, bool isHidden, CancellationToken cancellationToken = default);
}
