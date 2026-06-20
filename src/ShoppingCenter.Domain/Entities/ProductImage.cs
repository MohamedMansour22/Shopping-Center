namespace ShoppingCenter.Domain.Entities;

/// <summary>One image belonging to a product. Images are stored in the database (varbinary)
/// along with their MIME type; SortOrder gives a stable order, with 0 being the primary image
/// shown in the storefront grid, cart, and admin list.</summary>
public class ProductImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public byte[] ImageData { get; set; } = [];
    public string ImageContentType { get; set; } = string.Empty;

    // 0-based position; 0 is the primary image. Rewritten contiguously on every create/update.
    public int SortOrder { get; set; }
}
