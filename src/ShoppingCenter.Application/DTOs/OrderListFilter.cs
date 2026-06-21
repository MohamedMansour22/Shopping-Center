namespace ShoppingCenter.Application.DTOs;

/// <summary>Optional filters for the admin order list. Any null/blank field is ignored.
/// DateFrom/DateTo are absolute instants: the client converts the operator's chosen local calendar
/// days into UTC boundaries (DateFrom = start of the first day; DateTo = exclusive start of the day
/// after the last), so the range matches the operator's day rather than the UTC day.</summary>
public class OrderListFilter
{
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public string? CustomerName { get; set; }
    public int? StatusId { get; set; }
}
