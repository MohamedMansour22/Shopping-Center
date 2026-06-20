using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingCenter.Domain.Entities;
using ShoppingCenter.Infrastructure.Identity;

namespace ShoppingCenter.Infrastructure.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string AdminEmail = "admin@shop.local";
    // Local development credential only — change before any real deployment.
    public const string AdminPassword = "Admin#12345";

    /// <summary>Applies pending migrations and seeds the Admin role + default admin user + demo products.</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole(AdminRole));

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, AdminPassword);
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
            await userManager.AddToRoleAsync(admin, AdminRole);

        await SeedProductsAsync(db);
    }

    // Demo catalogue target size. The seeder tops the table up to this many products, so it
    // co-exists with any manually created products and is safe to re-run on every startup.
    private const int DemoProductCount = 200;

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        var existing = await db.Products.CountAsync();
        var toAdd = DemoProductCount - existing;
        if (toAdd <= 0)
            return;

        var categories = new (string Name, string[] Items, decimal BasePrice, string Bg)[]
        {
            ("Outerwear", new[] { "Coat", "Trench", "Overcoat", "Parka", "Blazer" }, 240m, "#e6e0d6"),
            ("Knitwear", new[] { "Sweater", "Cardigan", "Turtleneck", "Pullover" }, 160m, "#ece6dc"),
            ("Shirts", new[] { "Shirt", "Blouse", "Oxford Shirt", "Linen Shirt" }, 90m, "#eef0ee"),
            ("Trousers", new[] { "Trousers", "Chinos", "Wide-Leg Trousers", "Tailored Trousers" }, 120m, "#e9e4da"),
            ("Dresses", new[] { "Dress", "Slip Dress", "Midi Dress", "Wrap Dress" }, 180m, "#efe7e3"),
            ("Footwear", new[] { "Loafers", "Boots", "Derby Shoes", "Sneakers" }, 210m, "#e4ded4"),
            ("Bags", new[] { "Tote", "Shoulder Bag", "Crossbody Bag", "Clutch" }, 260m, "#e8e2d8"),
            ("Accessories", new[] { "Scarf", "Belt", "Gloves", "Hat" }, 60m, "#edeae3"),
        };
        var adjectives = new[]
        {
            "Classic", "Tailored", "Relaxed", "Heritage", "Essential",
            "Structured", "Draped", "Minimal", "Refined", "Everyday"
        };
        var materials = new[]
        {
            "Wool", "Cashmere", "Linen", "Cotton", "Silk", "Merino",
            "Leather", "Suede", "Charcoal", "Ivory", "Camel", "Navy", "Stone"
        };

        var now = DateTime.UtcNow;
        var products = new List<Product>(toAdd);

        for (var i = 0; i < toAdd; i++)
        {
            var cat = categories[i % categories.Length];
            var item = cat.Items[(i / categories.Length) % cat.Items.Length];
            var adjective = adjectives[(i * 3) % adjectives.Length];
            var material = materials[(i * 5) % materials.Length];

            var name = $"{adjective} {material} {item}";
            var price = cat.BasePrice + (i % 8) * 15m + 9m;
            // Every 12th item is out of stock to exercise the "Sold out" state.
            var stock = i % 12 == 0 ? 0 : 4 + i % 18;
            // A few products are hidden, to exercise admin-only visibility.
            var isHidden = i % 23 == 0;

            products.Add(new Product
            {
                Name = name,
                Description = $"{adjective} {item.ToLowerInvariant()} in {material.ToLowerInvariant()}. " +
                              $"A considered {cat.Name.ToLowerInvariant()} piece for the season.",
                Price = price,
                StockQuantity = stock,
                Category = cat.Name,
                Images = BuildPlaceholderImages(material, item, cat.Bg),
                IsHidden = isHidden,
                // Stagger timestamps so ordering is stable and varied.
                CreatedAtUtc = now.AddMinutes(-i)
            });
        }

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    // Three placeholder views per product (front / back / detail) so the detail-screen gallery has
    // several images out of the box. The primary (SortOrder 0) matches the old single-image look.
    private static readonly string[] ImageViews = { "Front", "Back", "Detail" };

    private static List<ProductImage> BuildPlaceholderImages(string material, string item, string bg)
    {
        return ImageViews
            .Select((view, i) => new ProductImage
            {
                ImageData = BuildPlaceholderSvg(material, item, bg, i == 0 ? null : view),
                ImageContentType = "image/svg+xml",
                SortOrder = i
            })
            .ToList();
    }

    // A lightweight 3:4 SVG placeholder (material label + item name on a warm tint), stored as a
    // product image so the storefront renders something real. An optional view label distinguishes
    // the secondary images in the gallery.
    private static byte[] BuildPlaceholderSvg(string material, string item, string bg, string? view)
    {
        var viewLabel = view is null
            ? string.Empty
            : $"<text x='150' y='262' font-family='Georgia, serif' font-size='12' letter-spacing='3' " +
              $"fill='#a8a196' text-anchor='middle'>{view.ToUpperInvariant()}</text>";

        var svg =
            $"<svg xmlns='http://www.w3.org/2000/svg' width='300' height='400' viewBox='0 0 300 400'>" +
            $"<rect width='300' height='400' fill='{bg}'/>" +
            $"<text x='150' y='188' font-family='Georgia, serif' font-size='14' letter-spacing='2' " +
            $"fill='#8a847b' text-anchor='middle'>{material.ToUpperInvariant()}</text>" +
            $"<text x='150' y='222' font-family='Georgia, serif' font-size='25' " +
            $"fill='#2b2925' text-anchor='middle'>{item}</text>" +
            viewLabel +
            $"</svg>";
        return Encoding.UTF8.GetBytes(svg);
    }
}
