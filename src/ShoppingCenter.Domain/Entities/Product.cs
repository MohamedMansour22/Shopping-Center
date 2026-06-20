namespace ShoppingCenter.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;

    // Up to 5 images per product (1-to-many). Ordered by ProductImage.SortOrder; index 0 is primary.
    public List<ProductImage> Images { get; set; } = new();

    // When true, the product is hidden from the customer storefront (admins still see it).
    public bool IsHidden { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
