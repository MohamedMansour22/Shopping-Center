using System.ComponentModel.DataAnnotations;

namespace ShoppingCenter.Application.DTOs;

public class CreateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    // Images to attach, in order (first becomes primary). Populated by the API from uploaded files.
    public List<ProductImageInput> Images { get; set; } = [];
}

public class UpdateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    // The desired final image set is expressed as: existing images to retain (KeepImageIds, in the
    // order they should appear) followed by NewImages (newly uploaded). Any existing image whose id
    // is not in KeepImageIds is deleted. An empty result set clears all images.
    public List<Guid> KeepImageIds { get; set; } = [];
    public List<ProductImageInput> NewImages { get; set; } = [];
}

// Raw image bytes + MIME type for one uploaded image (API maps uploaded files into these).
public class ProductImageInput
{
    public byte[] Data { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    // The image inlined as a data URI (e.g. "data:image/png;base64,...").
    public string ImageDataUri { get; set; } = string.Empty;
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool HasImage { get; set; }
    // Primary image (first by SortOrder) inlined as a data URI, or null. Kept so the storefront grid,
    // cart, and admin list can render one image straight from the products response.
    public string? ImageDataUri { get; set; }
    // All images for this product, ordered (primary first). Empty when the product has no images.
    public IReadOnlyList<ProductImageDto> Images { get; set; } = [];
    public bool IsHidden { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class SetVisibilityDto
{
    public bool IsHidden { get; set; }
}
