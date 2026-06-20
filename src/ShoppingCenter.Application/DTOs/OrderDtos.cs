using System.ComponentModel.DataAnnotations;

namespace ShoppingCenter.Application.DTOs;

// Posted by the storefront checkout. Prices are NOT trusted from the client — the server looks up
// each product and recomputes unit prices and totals.
public class CreateOrderDto
{
    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<OrderItemDto> Items { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
}

public class OrderStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateOrderStatusDto
{
    public int StatusId { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
