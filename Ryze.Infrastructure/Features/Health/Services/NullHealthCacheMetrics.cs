using Ryze.Application.Features.Health.Interfaces;

namespace Ryze.Infrastructure.Features.Health.Services;

/// <summary>
/// Provides noop implementation of health cache metrics.
/// </summary>
/// <remarks>
/// Used when cache telemetry is disabled or when no metrics collector
/// is configured. All operations intentionally perform no action while
/// preserving compatibility with components that depend on
/// <see cref="IHealthCacheMetrics"/>.
/// </remarks>
public sealed class NullHealthCacheMetrics : IHealthCacheMetrics
{
    /// <summary>
    /// Gets the shared singleton instance of the noop metrics collector.
    /// </summary>
    public static readonly NullHealthCacheMetrics Instance = new();

    /// <summary>
    /// Records cache hit event.
    /// </summary>
    public void RecordHit() { }

    /// <summary>
    /// Records stale cache hit event where expired data was served.
    /// </summary>
    public void RecordStaleHit() { }

    /// <summary>
    /// Records cache miss event.
    /// </summary>
    public void RecordMiss() { }

    /// <summary>
    /// Records fail open cache response where stale data was returned
    /// after refresh failure.
    /// </summary>
    public void RecordFailOpen() { }

    /// <summary>
    /// Records background cache refresh failure.
    /// </summary>
    /// <param name="ex">The exception that caused the refresh failure. </param>
    public void RecordBackgroundRefreshFailure(Exception ex) { }
}