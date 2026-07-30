namespace Ryze.Infrastructure.Features.Health.Options;

/// <summary>
/// Defines configuration options for periodic health audit execution.
/// </summary>
/// <remarks>
/// Controls the scheduling behavior of background health audit processes,
/// including the execution interval and startup jitter used to distribute
/// workload when multiple application instances start simultaneously.
/// </remarks>
public sealed class HealthAuditOptions
{
    /// <summary>
    /// Gets or sets the interval between health audit executions.
    /// </summary>
    /// <remarks>
    /// Determines how frequently the system health state is collected and
    /// evaluated by the background audit process.
    /// </remarks>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum random startup delay applied before the first
    /// health audit execution.
    /// </summary>
    /// <remarks>
    /// Startup jitter prevents synchronized health checks across multiple
    /// application instances, reducing load spikes against shared dependencies.
    /// </remarks>
    public TimeSpan StartupJitter { get; set; } = TimeSpan.FromSeconds(30);
}