namespace Ryze.Application.Shared.Pagination;

/// <summary>
/// Defines token-based pagination over an in memory source collection.
/// </summary>
public interface IPageTokenPagination
{
    /// <summary>
    /// Applies token-based pagination to the provided source collection.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="source">Source items to paginate.</param>
    /// <param name="pageSize">Requested maximum number of items per page.</param>
    /// <param name="pageToken">Opaque token representing current page position.</param>
    /// <returns>Slice of items and paging metadata.</returns>
    PageSlice<T> Apply<T>(
        IReadOnlyCollection<T> source,
        int pageSize,
        string? pageToken
    );
}
