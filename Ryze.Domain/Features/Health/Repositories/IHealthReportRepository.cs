using Ryze.Domain.Features.Health.ReadModels;

namespace Ryze.Domain.Features.Health.Repositories;

/// <summary>
/// Provides persistence operations for system health reports.
/// </summary>
/// <remarks>
/// Implementations are responsible for storing historical health reports and
/// exposing querying and retention operations used by monitoring, alerting,
/// and health audit processes.
/// </remarks>
public interface IHealthReportRepository
{
    /// <summary>
    /// Persists completed system health report.
    /// </summary>
    /// <param name="report">The health report containing the aggregated health state of the system. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    Task SaveAsync(
        SystemHealthReport report,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the most recently recorded system health report.
    /// </summary>
    /// <remarks>
    /// Returns the latest persisted report regardless of its health status,
    /// or <see langword="null"/> when no reports have been recorded.
    /// </remarks>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    Task<SystemHealthReport?> GetLatestAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the most recent system health reports.
    /// </summary>
    /// <remarks>
    /// Reports should be returned in descending chronological order,
    /// starting with the most recently recorded report.
    /// </remarks>
    /// <param name="limit">The maximum number of reports to return. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    Task<IReadOnlyList<SystemHealthReport>> ListAsync(
        int limit = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes health reports older than the specified cutoff timestamp.
    /// </summary>
    /// <remarks>
    /// Intended for retention policies that periodically remove historical
    /// health reports while preserving more recent monitoring history.
    /// </remarks>
    /// <param name="cutoff">Reports created before this timestamp are eligible for deletion. </param>
    /// <param name="batchSize">The maximum number of reports that may be removed during a single operation. </param>
    /// <param name="ct">The cancellation token used to cancel the operation. </param>
    /// <returns> The number of deleted health reports. </returns>
    Task<long> DeleteOlderThanAsync(
        DateTimeOffset cutoff,
        int batchSize = 500,
        CancellationToken ct = default);
}