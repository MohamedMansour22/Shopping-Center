namespace ShoppingCenter.Application.DTOs;

/// <summary>Which products a paged query should include, by storefront visibility.
/// Public storefront reads always use <see cref="VisibleOnly"/>; the admin list lets the
/// operator choose between all three.</summary>
public enum ProductVisibilityFilter
{
    All,         // hidden + visible
    VisibleOnly, // not hidden (the customer-facing set)
    HiddenOnly   // hidden only
}
