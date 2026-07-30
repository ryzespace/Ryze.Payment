namespace Ryze.Infrastructure.Features.Health.Options;

/// <summary>
/// Defines configuration options for health report retention and cleanup.
/// </summary>
/// <remarks>
/// Controls how frequently historical health reports are cleaned up and how
/// long completed health reports are retained before becoming eligible for
/// deletion.
///
/// These settings prevent unbounded growth of health monitoring history while
/// preserving enough data for operational analysis and diagnostics.
/// </remarks>
public sealed class HealthReportCleanupOptions
{
    /// <summary>
    /// Gets or sets the interval between cleanup executions.
    /// </summary>
    /// <remarks>
    /// Determines how often the background cleanup process checks for expired
    /// health reports.
    /// </remarks>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets or sets the retention period for stored health reports.
    /// </summary>
    /// <remarks>
    /// Reports older than this duration are eligible for removal during cleanup.
    /// </remarks>
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(30);
}