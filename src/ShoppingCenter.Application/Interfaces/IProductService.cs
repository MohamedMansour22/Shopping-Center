using ShoppingCenter.Application.DTOs;

namespace ShoppingCenter.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    // includeHidden is true only for admin callers; customers never see hidden products.
    Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeHidden, CancellationToken cancellationToken = default);

    // Storefront paged read: visible-only products with an optional name-OR-category search term.
    // Applies the merchandising policy of hiding sold-out products while browsing but surfacing them
    // when the customer is actively searching.
    Task<PagedResult<ProductDto>> GetStorefrontPageAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default);

    // Admin paged read: visibility selects which products to include by hidden state; name and
    // category are optional field filters (ANDed). Sold-out products stay visible for stock mgmt.
    Task<PagedResult<ProductDto>> GetAdminPageAsync(
        int page, int pageSize, ProductVisibilityFilter visibility, string? name, string? category,
        CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, bool includeHidden, CancellationToken cancellationToken = default);

    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetVisibilityAsync(Guid id, bool isHidden, CancellationToken cancellationToken = default);
}
