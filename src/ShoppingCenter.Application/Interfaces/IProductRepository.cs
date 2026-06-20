using ShoppingCenter.Domain.Entities;

namespace ShoppingCenter.Application.Interfaces;

public interface IProductRepository
{
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(bool includeHidden, CancellationToken cancellationToken = default);

    // Server-side pagination. Applies the visibility + search filter to BOTH the count and the
    // page slice, so TotalCount reflects the same set the items are drawn from.
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, bool includeHidden, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Loads the given products TRACKED (no images), so callers can mutate them (e.g. decrement stock)
    // and have the changes saved as part of the same unit of work. Missing ids are simply absent.
    Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    // Applies scalar changes from <paramref name="product"/> and reconciles its images: existing
    // images whose id is in keepImageIds are retained (re-ordered by that list), others are deleted,
    // and newImages are appended. Returns the updated product (with images), or null if not found.
    Task<Product?> UpdateAsync(
        Product product,
        IReadOnlyList<Guid> keepImageIds,
        IReadOnlyList<ProductImage> newImages,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Targeted single-column update; leaves the product's images untouched.
    Task<bool> SetVisibilityAsync(Guid id, bool isHidden, CancellationToken cancellationToken = default);
}
