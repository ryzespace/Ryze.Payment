using Marten;
using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Repositories;
using Ryze.Infrastructure.Features.Ledger.Repositories.Redis;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Marten;

/// <summary>
/// Provides Marten-backed persistence operations for ledger accounting periods.
/// </summary>
/// <remarks>
/// Uses Marten as the persistence store for ledger periods and process local
/// Redis invalidated cache for efficient period lookups by timestamp.
///
/// The cached period collection is ordered by <see cref="LedgerPeriod.StartsAt"/>
/// and searched using binary search when resolving the period containing a
/// specified timestamp.
///
/// Cache invalidation is propagated across application instances through
/// <see cref="RedisLedgerPeriodCache"/>, ensuring that changes to ledger periods
/// are reflected by later cached reads across the distributed application.
/// </remarks>
public sealed class MartenLedgerPeriodRepository(
    IDocumentSession session,
    RedisLedgerPeriodCache cache) : ILedgerPeriodRepository
{
    /// <summary>
    /// Returns the accounting period that contains the specified timestamp.
    /// </summary>
    /// <remarks>
    /// Periods are loaded from the cache and searched by their start timestamp.
    /// The selected period must contain the timestamp within the half-open interval
    /// defined by <see cref="LedgerPeriod.StartsAt"/> and <see cref="LedgerPeriod.EndsAt"/>.
    ///
    /// The lookup uses binary search over the chronologically ordered period
    /// collection, providing efficient resolution without querying the database
    /// for every request.
    /// </remarks>
    /// <param name="timestamp">The timestamp for which the containing accounting period is requested.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous cache loading operation.</param>
    public async Task<LedgerPeriod?> GetPeriodForDateAsync(
        DateTimeOffset timestamp,
        CancellationToken ct = default)
    {
        var periods = await cache.GetOrLoadAsync(LoadPeriodsAsync, ct);

        if (periods.Count == 0)
            return null;

        var candidateIndex = FindLastIndexWithStartsAtLessOrEqual(periods, timestamp);

        if (candidateIndex < 0)
            return null;

        var candidate = periods[candidateIndex];

        return timestamp >= candidate.StartsAt && timestamp < candidate.EndsAt
            ? candidate
            : null;
    }

    /// <summary>
    /// Returns an accounting period by its unique identifier.
    /// </summary>
    /// <remarks>
    /// This operation loads the period directly from Marten and does not use
    /// the period collection cache.
    /// </remarks>
    /// <param name="periodId">The unique identifier of the accounting period.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous database operation.</param>
    public async Task<LedgerPeriod?> GetByIdAsync(
        string periodId,
        CancellationToken ct = default) =>
        await session.LoadAsync<LedgerPeriod>(periodId, ct);

    /// <summary>
    /// Persists new or updated accounting period and invalidates cached period data.
    /// </summary>
    /// <remarks>
    /// The period is stored through the current Marten document session and the
    /// session changes are persisted before the distributed cache invalidation
    /// is published.
    ///
    /// Cache invalidation ensures that the current process and other application
    /// instances do not continue using a stale collection of ledger periods.
    /// </remarks>
    /// <param name="period">The accounting period to persist.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous persistence operation.</param>
    public async Task SaveAsync(
        LedgerPeriod period,
        CancellationToken ct = default)
    {
        session.Store(period);
        await session.SaveChangesAsync(ct);

        await cache.InvalidateAsync();
    }

    /// <summary>
    /// Returns all accounting periods ordered by their start timestamp.
    /// </summary>
    /// <remarks>
    /// Results are retrieved from the shared period cache when available.
    /// When the cache is empty, the periods are loaded from Marten, ordered
    /// chronologically, and stored in the cache for later lookups.
    /// </remarks>
    /// <param name="ct">Token that can be used to cancel the asynchronous cache loading operation.</param>
    /// <returns>
    public async Task<IReadOnlyList<LedgerPeriod>> GetAllAsync(
        CancellationToken ct = default) =>
        await cache.GetOrLoadAsync(LoadPeriodsAsync, ct);

    /// <summary>
    /// Loads all accounting periods from Marten in chronological order.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the asynchronous database query.</param>
    private async Task<IReadOnlyList<LedgerPeriod>> LoadPeriodsAsync(
        CancellationToken ct) =>
        await session.Query<LedgerPeriod>()
            .OrderBy(p => p.StartsAt)
            .ToListAsync(ct);

    /// <summary>
    /// Finds the index of the last accounting period whose start timestamp
    /// is less than or equal to the specified timestamp.
    /// </summary>
    /// <remarks>
    /// Uses binary search over collection ordered by
    /// <see cref="LedgerPeriod.StartsAt"/> in ascending order.
    ///
    /// The returned index identifies the only possible period that can contain
    /// the timestamp. The caller is responsible for verifying the period's
    /// ending boundary.
    /// </remarks>
    /// <param name="periods">The chronologically ordered collection of accounting periods.</param>
    /// <param name="timestamp">The timestamp used to locate the candidate accounting period.</param>
    /// <returns>
    /// The index of the last period whose start timestamp is less than or equal
    /// to the specified timestamp, or <c>-1</c> when no such period exists.
    /// </returns>
    private static int FindLastIndexWithStartsAtLessOrEqual(
        IReadOnlyList<LedgerPeriod> periods,
        DateTimeOffset timestamp)
    {
        var lo = 0;
        var hi = periods.Count - 1;
        var result = -1;

        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;

            if (periods[mid].StartsAt <= timestamp)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result;
    }
}