namespace Ryze.Infrastructure.Features.Health.Services;

/// <summary>
/// Defines caching behavior for health status reports.
/// </summary>
/// <remarks>
/// Controls cache expiration, stale data handling, refresh jitter,
/// and fail open behavior used to balance health endpoint availability
/// with the freshness of reported health information.
/// </remarks>
public sealed class HealthStatusCacheOptions
{
    /// <summary>
    /// Gets the duration for which cached health report is considered fresh.
    /// </summary>
    public TimeSpan Ttl { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the additional period during which stale cached data may be used
    /// while attempting to refresh the health report.
    /// </summary>
    public TimeSpan StaleWindow { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets the maximum random delay applied during cache refresh scheduling.
    /// </summary>
    /// <remarks>
    /// Jitter helps prevent multiple instances from refreshing cached health
    /// data simultaneously.
    /// </remarks>
    public TimeSpan JitterRange { get; init; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets value indicating whether stale cache data may be returned when
    /// refreshing the health report fails.
    /// </summary>
    /// <remarks>
    /// When enabled, the system prioritizes availability of the health endpoint
    /// over strict freshness guarantees. Cached data may be served after hard
    /// expiration if it remains within the configured <see cref="FailOpenMaxAge"/>.
    /// </remarks>
    public bool FailOpen { get; init; } = true;

    /// <summary>
    /// Gets the maximum age of cached health data that may be served during
    /// fail open mode.
    /// </summary>
    /// <remarks>
    /// Prevents returning health information that is too old to be meaningful.
    /// Once the cached report exceeds this limit, refresh failures are propagated
    /// instead of returning stale data.
    /// </remarks>
    public TimeSpan FailOpenMaxAge { get; init; } = TimeSpan.FromMinutes(5);
}