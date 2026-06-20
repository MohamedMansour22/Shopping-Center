namespace ShoppingCenter.Application.DTOs;

/// <summary>A single page of results plus the metadata a client needs to request the next one.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    // True when more pages remain after this one (i.e. Page * PageSize < TotalCount).
    public bool HasMore { get; set; }
}
