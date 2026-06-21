using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;

namespace ShoppingCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // Public: the storefront checkout places an order (no account required).
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto, CancellationToken ct)
    {
        var (order, error) = await _orderService.CreateAsync(dto, ct);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetAll), new { id = order!.Id }, order);
    }

    // Admin-only: list placed orders (newest first), server-side paginated.
    // Optional filters: date range (dateFrom/dateTo), customer name (substring), and status id.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] DateTimeOffset? dateFrom = null,
        [FromQuery] DateTimeOffset? dateTo = null,
        [FromQuery] string? customerName = null,
        [FromQuery] int? statusId = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var filter = new OrderListFilter
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            CustomerName = customerName,
            StatusId = statusId
        };
        var orders = await _orderService.GetPagedAsync(page, pageSize, filter, ct);
        return Ok(orders);
    }

    // Admin-only: a single order with its items and status (order details screen).
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    // Admin-only: the order status lookup (Placed, Delivered) for status dropdowns.
    [HttpGet("statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<OrderStatusDto>>> GetStatuses(CancellationToken ct)
    {
        var statuses = await _orderService.GetStatusesAsync(ct);
        return Ok(statuses);
    }

    // Admin-only: change an order's status.
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusDto dto, CancellationToken ct)
    {
        var (order, error) = await _orderService.UpdateStatusAsync(id, dto.StatusId, ct);
        if (error == "NotFound")
            return NotFound();
        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(order);
    }
}
