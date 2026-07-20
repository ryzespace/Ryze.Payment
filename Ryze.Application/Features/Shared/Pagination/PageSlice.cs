namespace Ryze.Application.Features.Shared.Pagination;

/// <summary>
/// Represents one paginated result page and its metadata.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
/// <param name="Items">Items included in current page.</param>
/// <param name="NextPageToken">Opaque token for fetching next page; <see langword="null"/> when no next page exists.</param>
/// <param name="TotalCount">Total number of items in a full source collection.</param>
public sealed record PageSlice<T>(
    IReadOnlyList<T> Items,
    string? NextPageToken,
    int TotalCount
);
