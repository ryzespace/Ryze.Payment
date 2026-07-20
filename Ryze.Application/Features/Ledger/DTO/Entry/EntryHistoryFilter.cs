using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.DTO.Entry;

/// <summary>
/// Represents filtering and ordering criteria for ledger entry history queries.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of query parameters
/// used to retrieve ledger entries.
///
/// Supports filtering by account, date range, amount range, entry status,
/// direction, and configurable ordering with pagination controls.
/// </remarks>
/// <param name="AccountId">
/// Ledger account identifier used as the primary filter scope.
/// </param>
/// <param name="DateFrom">Optional lower timestamp boundary for entry history filtering.</param>
/// <param name="DateTo">Optional upper timestamp boundary for entry history filtering.</param>
/// <param name="AmountMin">Optional minimum entry amount filter.</param>
/// <param name="AmountMax">Optional maximum entry amount filter.</param>
/// <param name="Status">Optional ledger entry status filter.</param>
/// <param name="Direction">Optional ledger entry direction filter, such as debit or credit.</param>
/// <param name="SortBy">Field name used for ordering the returned entries.</param>
/// <param name="SortDirection">Ordering direction, such as ascending or descending.</param>
/// <param name="Skip">Number of entries to skip before returning results.</param>
/// <param name="Take">Maximum number of entries to return./param>
public sealed record EntryHistoryFilter(
    string AccountId,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null,
    decimal? AmountMin = null,
    decimal? AmountMax = null,
    EntryStatus? Status = null,
    string? Direction = null,
    string SortBy = "Timestamp",
    string SortDirection = "Desc",
    int Skip = 0,
    int Take = 50
);
