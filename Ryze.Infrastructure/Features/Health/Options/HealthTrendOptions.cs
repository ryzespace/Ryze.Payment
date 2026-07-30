namespace Ryze.Infrastructure.Features.Health.Options;

/// <summary>
/// Defines configuration options for health trend analysis.
/// </summary>
/// <remarks>
/// Controls the execution schedule and processing limits for background health
/// trend calculations, including sampling frequency, startup distribution,
/// report processing boundaries, and availability evaluation thresholds.
///
/// These settings allow operational health analysis to run periodically while
/// preventing excessive resource usage when processing large health histories.
/// </remarks>
public sealed class HealthTrendOptions
{
    /// <summary>
    /// Gets or sets the interval between health trend analysis executions.
    /// </summary>
    /// <remarks>
    /// Determines how frequently historical health reports are aggregated
    /// into trend information.
    /// </remarks>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the maximum random startup delay before the first trend
    /// analysis execution.
    /// </summary>
    /// <remarks>
    /// Startup jitter prevents multiple application instances from performing
    /// trend calculations simultaneously after deployment or restart.
    /// </remarks>
    public TimeSpan StartupJitter { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum number of health reports processed within
    /// single trend calculation window.
    /// </summary>
    /// <remarks>
    /// Limits the amount of historical data processed during one execution to
    /// protect application resources when health report history grows.
    /// </remarks>
    public int MaxReportsPerWindow { get; set; } = 10_000;

    /// <summary>
    /// Gets or sets the minimum availability percentage considered acceptable
    /// during health trend evaluation.
    /// </summary>
    /// <remarks>
    /// Used as threshold when calculating whether observed system availability
    /// meets the expected operational target.
    /// </remarks>
    public double AvailabilityThresholdPercent { get; set; } = 95.0;
}