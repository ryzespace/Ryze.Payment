namespace Ryze.Infrastructure.Features.Health.Options;

/// <summary>
/// Defines configuration options for health alert monitoring.
/// </summary>
/// <remarks>
/// Controls the execution frequency of health alert evaluation, the number of
/// consecutive failures required before escalating an alert, and the lifetime
/// of inactive failure tracking entries.
/// These settings allow health monitoring to avoid noisy alerts caused by
/// transient failures while still detecting persistent component degradation.
/// </remarks>
public sealed class HealthAlertOptions
{
    /// <summary>
    /// Gets or sets the interval between health alert evaluation cycles.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the number of consecutive unhealthy checks required before
    /// treating component failure as persistent.
    /// </summary>
    /// <remarks>
    /// Used to reduce alert noise by ignoring isolated health check failures.
    /// </remarks>
    public int ConsecutiveFailureThreshold { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum lifetime of inactive failure tracking entries.
    /// </summary>
    /// <remarks>
    /// Entries that have not been observed within this period are removed to
    /// prevent stale component failure state from accumulating in memory.
    /// </remarks>
    public TimeSpan StaleEntryLifetime { get; set; } = TimeSpan.FromHours(1);
}