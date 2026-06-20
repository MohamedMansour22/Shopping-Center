using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoppingCenter.Domain.Entities;
using ShoppingCenter.Infrastructure.Identity;

namespace ShoppingCenter.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Category).HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            // Deleting a product cascades to its images (DeleteAsync relies on this).
            entity.HasMany(p => p.Images)
                .WithOne(i => i.Product!)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductImage>(entity =>
        {
            entity.Property(i => i.ImageData).IsRequired();
            entity.Property(i => i.ImageContentType).IsRequired().HasMaxLength(100);
            entity.HasIndex(i => new { i.ProductId, i.SortOrder });
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(256);
            entity.Property(o => o.CustomerPhone).IsRequired().HasMaxLength(40);
            entity.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.StatusId).HasDefaultValue(OrderStatusIds.Placed);
            entity.HasIndex(o => o.CreatedAtUtc);

            // Deleting an order cascades to its line items.
            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order!)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Status lookup FK; Restrict so a referenced status can't be deleted out from under orders.
            entity.HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderStatus>(entity =>
        {
            // Fixed lookup rows: ids are assigned explicitly, not generated.
            entity.Property(s => s.Id).ValueGeneratedNever();
            entity.Property(s => s.Name).IsRequired().HasMaxLength(50);
            entity.HasData(
                new OrderStatus { Id = OrderStatusIds.Placed, Name = OrderStatusIds.NameOf(OrderStatusIds.Placed) },
                new OrderStatus { Id = OrderStatusIds.Delivered, Name = OrderStatusIds.NameOf(OrderStatusIds.Delivered) });
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            // LineTotal is computed in code (UnitPrice * Quantity); don't map it to a column.
            entity.Ignore(i => i.LineTotal);
        });

        builder.Entity<DeviceToken>(entity =>
        {
            // Bounded so the unique index stays within SQL Server's key-size limit; FCM tokens are ~200 chars.
            entity.Property(t => t.Token).IsRequired().HasMaxLength(450);
            entity.Property(t => t.UserId).IsRequired().HasMaxLength(450);
            // FCM tokens are the natural key for upsert/prune; keep them unique.
            entity.HasIndex(t => t.Token).IsUnique();
        });

        builder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.Type).IsRequired().HasMaxLength(50);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
            // Feed queries order by recency and filter unread for the badge count.
            entity.HasIndex(n => n.CreatedAtUtc);
            entity.HasIndex(n => n.IsRead);
        });
    }
}
