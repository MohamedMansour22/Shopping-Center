using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;

namespace ShoppingCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB
    private const int MaxImagesPerProduct = 5;
    private const int DefaultPageSize = 12;
    private const int AdminPageSize = 20;
    private const int MaxPageSize = 48;

    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // --- Public storefront reads: ALWAYS visible-only, regardless of any token sent. ---
    // (The admin UI attaches its JWT to every request, so role can't be trusted to hide products here.)

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        // The service owns the browse-vs-search merchandising policy (hide sold-out while browsing).
        var result = await _productService.GetStorefrontPageAsync(page, pageSize, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, includeHidden: false, ct);
        return product is null ? NotFound() : Ok(product);
    }

    // --- Admin reads: include hidden products (admin-only). ---

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAllForAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = AdminPageSize,
        [FromQuery] ProductVisibilityFilter visibility = ProductVisibilityFilter.All,
        [FromQuery] string? name = null,
        [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        // The admin list can show all products, only visible, or only hidden (operator's choice via
        // the visibility filter), and can narrow by name and/or category; sold-out products stay
        // visible for stock mgmt.
        var result = await _productService.GetAdminPageAsync(page, pageSize, visibility, name, category, ct);
        return Ok(result);
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> GetByIdForAdmin(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, includeHidden: true, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductDto>> Create([FromForm] CreateProductForm form, CancellationToken ct)
    {
        var (images, error) = await ReadImagesAsync(form.Images, ct);
        if (error is not null)
            return BadRequest(new { message = error });

        var dto = new CreateProductDto
        {
            Name = form.Name,
            Description = form.Description ?? string.Empty,
            Price = form.Price,
            StockQuantity = form.StockQuantity,
            Category = form.Category ?? string.Empty,
            Images = images
        };

        var created = await _productService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromForm] UpdateProductForm form, CancellationToken ct)
    {
        var keepImageIds = form.KeepImageIds ?? [];

        var (newImages, error) = await ReadImagesAsync(form.Images, ct);
        if (error is not null)
            return BadRequest(new { message = error });

        // The final set is kept-existing + new-uploads; enforce the per-product cap across both.
        if (keepImageIds.Count + newImages.Count > MaxImagesPerProduct)
            return BadRequest(new { message = $"A product can have at most {MaxImagesPerProduct} images." });

        var dto = new UpdateProductDto
        {
            Name = form.Name,
            Description = form.Description ?? string.Empty,
            Price = form.Price,
            StockQuantity = form.StockQuantity,
            Category = form.Category ?? string.Empty,
            KeepImageIds = keepImageIds,
            NewImages = newImages
        };

        var updated = await _productService.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _productService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    // Toggle whether a product is hidden from the customer storefront.
    [HttpPut("{id:guid}/visibility")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetVisibility(Guid id, SetVisibilityDto dto, CancellationToken ct)
    {
        var updated = await _productService.SetVisibilityAsync(id, dto.IsHidden, ct);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Validates and reads uploaded images. Returns (images, error); error is non-null on the
    /// first invalid file or if more than the allowed number of images are supplied.</summary>
    private static async Task<(List<ProductImageInput> Images, string? Error)> ReadImagesAsync(
        List<IFormFile>? files, CancellationToken ct)
    {
        var images = new List<ProductImageInput>();
        if (files is null || files.Count == 0)
            return (images, null);

        if (files.Count > MaxImagesPerProduct)
            return (images, $"A product can have at most {MaxImagesPerProduct} images.");

        foreach (var file in files)
        {
            if (file is not { Length: > 0 })
                continue;

            if (file.Length > MaxImageBytes)
                return (images, "Each image must be 5 MB or smaller.");

            if (string.IsNullOrEmpty(file.ContentType) ||
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return (images, "Each uploaded file must be an image.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            images.Add(new ProductImageInput { Data = ms.ToArray(), ContentType = file.ContentType });
        }

        return (images, null);
    }
}

/// <summary>Multipart form bound on product creation (kept in the API layer because of IFormFile).</summary>
public class CreateProductForm
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    // Newly uploaded image files (up to 5). On create these are the product's images, in order.
    public List<IFormFile>? Images { get; set; }
}

/// <summary>Multipart form bound on product update. KeepImageIds lists the existing images to retain
/// (in display order); anything not listed is removed. Images carries any newly uploaded files.</summary>
public class UpdateProductForm : CreateProductForm
{
    public List<Guid>? KeepImageIds { get; set; }
}
