namespace Ryze.Infrastructure.Features.Health.Options;

/// <summary>
/// Defines configuration options for health status caching behavior.
/// </summary>
/// <remarks>
/// Controls stale while revalidate caching semantics for health reports,
/// including cache freshness duration, stale serving window, refresh jitter,
/// and fail open behavior during refresh failures.
///
/// These options allow balancing health endpoint responsiveness and availability
/// against the freshness requirements of monitoring data.
/// </remarks>
public sealed class HealthStatusCacheOptions
{
    /// <summary>
    /// Gets or sets the duration for which cached health report is considered fresh.
    /// </summary>
    /// <remarks>
    /// During this period, health requests are served directly from cache without
    /// executing a new health collection operation.
    /// </remarks>
    public TimeSpan Ttl { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the period during which stale cache data may be served while
    /// a background refresh is triggered.
    /// </summary>
    /// <remarks>
    /// Provides stale-while-revalidate behavior by allowing callers to receive
    /// the previous health state without waiting for refresh completion.
    /// </remarks>
    public TimeSpan StaleWindow { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the maximum random delay applied during cache expiration
    /// calculations.
    /// </summary>
    /// <remarks>
    /// Jitter reduces synchronized cache refreshes across multiple application
    /// instances by spreading refresh operations over time.
    /// </remarks>
    public TimeSpan JitterRange { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets value indicating whether stale health reports may be returned
    /// when refreshing the cache fails.
    /// </summary>
    /// <remarks>
    /// When enabled, the system prioritizes health endpoint availability over
    /// strict data freshness by serving previously cached reports within the
    /// configured <see cref="FailOpenMaxAge"/> limit.
    /// </remarks>
    public bool FailOpen { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum age of cached health data that may be served
    /// during fail open mode.
    /// </summary>
    /// <remarks>
    /// Prevents returning health information that is too old to provide
    /// meaningful operational insight. Once exceeded, refresh failures are
    /// propagated instead of returning stale data.
    /// </remarks>
    public TimeSpan FailOpenMaxAge { get; set; } = TimeSpan.FromMinutes(5);
}