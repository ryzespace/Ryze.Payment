namespace Ryze.Application.Features.Health.Interfaces;

/// <summary>
/// Defines metrics operations for health report caching behavior.
/// </summary>
/// <remarks>
/// Implementations are responsible for recording cache-related telemetry,
/// including cache efficiency, stale data usage, fail-open scenarios,
/// and refresh failures.
/// </remarks>
public interface IHealthCacheMetrics
{
    /// <summary>
    /// Records successful cache hit where fresh cached value was returned.
    /// </summary>
    void RecordHit();

    /// <summary>
    /// Records stale cache hit where an expired value was served during
    /// the stale while revalidate window.
    /// </summary>
    void RecordStaleHit();

    /// <summary>
    /// Records cache miss requiring data retrieval from the underlying source.
    /// </summary>
    void RecordMiss();

    /// <summary>
    /// Records fail open cache response where stale data was returned after
    /// refresh failure to preserve service availability.
    /// </summary>
    void RecordFailOpen();

    /// <summary>
    /// Records failure during background cache refresh execution.
    /// </summary>
    /// <param name="ex">
    /// The exception that caused the refresh failure.
    /// </param>
    void RecordBackgroundRefreshFailure(Exception ex);
}