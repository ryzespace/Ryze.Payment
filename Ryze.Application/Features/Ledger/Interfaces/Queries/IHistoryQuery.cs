using Ryze.Application.Features.Ledger.DTO.Entry;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Interfaces.Queries;

/// <summary>
/// Defines read-only operations for querying ledger entry history.
/// </summary>
/// <remarks>
/// Provides queries for retrieving ledger entry history
/// with support for filtering, ordering, and scalable pagination.
/// </remarks>
public interface IHistoryQuery
{
    /// <summary>
    /// Queries ledger entry history using the specified filtering criteria.
    /// </summary>
    /// <param name="filter">Entry history filtering and ordering criteria.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching ledger entry history result.</returns>
    Task<EntryHistoryResultDto> QueryEntryHistoryAsync(
        EntryHistoryFilter filter,
        CancellationToken ct = default);

    /// <summary>
    /// Queries ledger entry history using continuation-token pagination.
    /// </summary>
    /// <remarks>
    /// Uses keyset pagination to provide stable and efficient traversal
    /// of large ledger datasets without relying on offset-based paging.
    /// </remarks>
    /// <returns>Paginated ledger entry history result with continuation information.</returns>
    Task<EntryHistoryTokenResultDto> QueryEntryHistoryWithTokenAsync(
        string accountId,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        decimal? amountMin = null,
        decimal? amountMax = null,
        EntryStatus? status = null,
        string? direction = null,
        string sortBy = "Timestamp",
        string sortDirection = "Desc",
        int limit = 50,
        string? continuationToken = null,
        CancellationToken ct = default);
}
