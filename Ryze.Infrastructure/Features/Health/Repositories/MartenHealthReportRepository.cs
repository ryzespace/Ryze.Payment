using Marten;
using Ryze.Domain.Features.Health.ReadModels;
using Ryze.Domain.Features.Health.Repositories;

namespace Ryze.Infrastructure.Features.Health.Repositories;

/// <summary>
/// Provides Marten based persistence for system health reports.
/// </summary>
/// <remarks>
/// Stores health reports as Marten documents and exposes querying operations
/// required by monitoring, diagnostics, historical analysis, and retention
/// workflows.
///
/// IMPORTANT: <see cref="SystemHealthReport.CheckedAt"/> must have duplicated
/// column index configured where the Marten <c>DocumentStore</c> is registered,
/// Every query in this repository filters or sorts by CheckedAt; without the
/// index, all of them fall back to JSONB extraction scans instead of an
/// index scan, and the cost grows unbounded as the table accumulates rows.
/// </remarks>
/// <param name="session">
/// The Marten document session used for health report persistence and queries.
/// </param>
public sealed class MartenHealthReportRepository(
    IDocumentSession session) : IHealthReportRepository
{
    /// <summary>
    /// Default cap applied to <see cref="GetByPeriodAsync"/> when the caller
    /// does not supply an explicit limit, to prevent unbounded materialization
    /// of large date ranges into memory.
    /// </summary>
    private const int DefaultPeriodResultCap = 1000;

    /// <summary>
    /// Delay applied between delete batches in <see cref="DeleteOlderThanAsync"/>
    /// to avoid sustained back to back write pressure on the database when
    /// large backlog is being cleared (eg. after an extended retention gap).
    /// </summary>
    private static readonly TimeSpan DeleteBatchThrottle = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Persists completed system health report.
    /// </summary>
    /// <param name="report">The health report to store. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    public async Task SaveAsync(
        SystemHealthReport report,
        CancellationToken ct = default)
    {
        session.Store(report);
        await session.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Retrieves the latest recorded system health report.
    /// </summary>
    /// <remarks>
    /// Returns the most recently completed health evaluation based on the
    /// <see cref="SystemHealthReport.CheckedAt"/> timestamp.
    /// </remarks>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    /// <returns>
    /// The newest available health report, or <see langword="null"/> when
    /// no reports exist.
    /// </returns>
    public async Task<SystemHealthReport?> GetLatestAsync(
        CancellationToken ct = default)
    {
        return await session.Query<SystemHealthReport>()
            .OrderByDescending(r => r.CheckedAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Retrieves recent system health reports.
    /// </summary>
    /// <remarks>
    /// Reports are returned in reverse chronological order, allowing callers
    /// to inspect recent health history.
    /// </remarks>
    /// <param name="limit">Maximum number of reports to return. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    public async Task<IReadOnlyList<SystemHealthReport>> ListAsync(
        int limit = 10,
        CancellationToken ct = default)
    {
        return await session.Query<SystemHealthReport>()
            .OrderByDescending(r => r.CheckedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Retrieves health reports within specified time range.
    /// </summary>
    /// <remarks>
    /// Supports historical health analysis by limiting results to reports
    /// created between the provided timestamps. When <paramref name="limit"/>
    /// is not supplied, a default cap of <see cref="DefaultPeriodResultCap"/>
    /// is applied to avoid materializing an unbounded result set for wide
    /// date ranges.
    /// </remarks>
    /// <param name="from">The beginning of the requested time range. </param>
    /// <param name="to">The end of the requested time range.</param>
    /// <param name="limit">Optional maximum number of reports to return. Defaults to <see cref="DefaultPeriodResultCap"/> when omitted.</param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    public Task<IReadOnlyList<SystemHealthReport>> GetByPeriodAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int? limit = null,
        CancellationToken ct = default)
    {
        var effectiveLimit = limit ?? DefaultPeriodResultCap;

        return session.Query<SystemHealthReport>()
            .Where(r =>
                r.CheckedAt >= from &&
                r.CheckedAt <= to)
            .OrderByDescending(r => r.CheckedAt)
            .Take(effectiveLimit)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Counts stored health reports optionally limited by creation time.
    /// </summary>
    /// <param name="since">Optional timestamp restricting the count to newer reports. </param>
    /// <param name="ct">the cancellation token used to cancel the operation. </param>
    public Task<long> CountAsync(
        DateTimeOffset? since = null,
        CancellationToken ct = default)
    {
        if (since.HasValue)
        {
            return session.Query<SystemHealthReport>()
                .Where(r => r.CheckedAt >= since.Value)
                .LongCountAsync(ct);
        }

        return session.Query<SystemHealthReport>()
            .LongCountAsync(ct);
    }

    /// <summary>
    /// Deletes health reports older than the specified retention boundary.
    /// </summary>
    /// <remarks>
    /// Deletes documents in batches to avoid loading large amounts of historical
    /// health data into memory and to reduce transaction size. Only document
    /// identifiers are fetched per batch — full JSONB payloads are never
    /// materialized just to be deleted. A short delay is applied between full
    /// batches to avoid sustained write pressure when clearing a large backlog.
    /// </remarks>
    /// <param name="cutoff">Reports older than this timestamp are removed. </param>
    /// <param name="batchSize">Maximum number of reports removed per iteration. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    /// <returns>Total number of removed health reports. </returns>
    public async Task<long> DeleteOlderThanAsync(
        DateTimeOffset cutoff,
        int batchSize = 500,
        CancellationToken ct = default)
    {
        var totalDeleted = 0L;

        while (!ct.IsCancellationRequested)
        {
            var ids = await session.Query<SystemHealthReport>()
                .Where(r => r.CheckedAt < cutoff)
                .Take(batchSize)
                .Select(r => r.Id)
                .ToListAsync(ct);

            if (ids.Count == 0)
                break;

            foreach (var id in ids)
                session.Delete<SystemHealthReport>(id);

            await session.SaveChangesAsync(ct);
            totalDeleted += ids.Count;

            // Only pause when the batch was full — a partial batch means this
            // was the last iteration, so there's no point delaying before exit.
            if (ids.Count == batchSize)
                await Task.Delay(DeleteBatchThrottle, ct);
        }

        return totalDeleted;
    }
}