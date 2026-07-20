namespace Ryze.Application.Features.Ledger.DTO.Entry;

/// <summary>
/// Represents the result of ledger entry history query.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of a paginated
/// ledger entry history response.
///
/// Contains the returned ledger entries together with pagination metadata
/// required for navigating and presenting the query results.
/// </remarks>
/// <param name="Items">Collection of ledger entries matching the requested query criteria.</param>
/// <param name="TotalCount">Total number of matching ledger entries available.</param>
/// <param name="Skip">Number of entries skipped before the current result set.</param>
/// <param name="Take">Maximum number of entries requested for the current result set.</param>
public sealed record EntryHistoryResultDto(
    IReadOnlyList<EntryItemDto> Items,
    int TotalCount,
    int Skip,
    int Take
);
