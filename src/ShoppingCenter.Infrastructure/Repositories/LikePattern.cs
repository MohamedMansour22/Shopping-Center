namespace ShoppingCenter.Infrastructure.Repositories;

/// <summary>Helpers for building safe SQL <c>LIKE</c> patterns from user input.</summary>
internal static class LikePattern
{
    /// <summary>Builds a "contains" pattern (<c>%term%</c>) for use with
    /// <see cref="Microsoft.EntityFrameworkCore.EF.Functions"/>.Like, with SQL Server's LIKE
    /// wildcard characters (<c>%</c>, <c>_</c>, <c>[</c>) escaped via bracket-quoting so the term
    /// is matched literally. Without this, a term like "50%" or "A_B" would be treated as a
    /// wildcard pattern and over-match, and a lone "%" would match every row.</summary>
    public static string Contains(string term)
    {
        // Escape '[' first: the '%'/'_' replacements introduce '[' characters that must not be
        // re-escaped.
        var escaped = term
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
        return $"%{escaped}%";
    }
}
