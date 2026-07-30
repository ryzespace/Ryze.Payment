using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ryze.Application.Features.Health.Interfaces;
using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.ReadModels;
using Ryze.Infrastructure.Features.Health.Options;

namespace Ryze.Infrastructure.Features.Health.Background;

/// <summary>
/// Background worker responsible for periodically collecting system health state
/// and persisting health audit reports.
/// </summary>
/// <remarks>
/// Executes health checks on configurable interval and stores completed reports
/// through <see cref="IHealthCheckCollector"/>.
/// Startup jitter is intentionally applied before the first execution to prevent
/// synchronized health checks across multiple application instances starting at
/// the same time.
/// The worker does not expose health state directly. Its responsibility is to
/// create durable health history used by alerting, diagnostics, and trend analysis.
/// </remarks>
/// <param name="serviceProvider">Service provider used to create scoped dependencies required during health audit. </param>
/// <param name="logger"> Logger used for health audit execution diagnostics. </param>
/// <param name="options"> Configuration controlling audit interval and startup jitter. </param>
public sealed class HealthAuditBackgroundWorker(
    IServiceProvider serviceProvider,
    ILogger<HealthAuditBackgroundWorker> logger,
    IOptions<HealthAuditOptions> options) : BackgroundService
{
    private readonly HealthAuditOptions _options = options.Value;

    /// <summary>
    /// Starts the periodic health audit execution loop.
    /// </summary>
    /// <remarks>
    /// A randomized startup delay is introduced before creating the periodic timer.
    /// This avoids thundering herd behavior when multiple application instances
    /// become available simultaneously.
    ///
    /// Cancellation during shutdown is treated as an expected lifecycle event and
    /// does not produce an error log.
    /// </remarks>
    /// <param name="stoppingToken">Token used to gracefully stop the background worker. </param>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Health Audit Background Worker started.");

        await ApplyStartupJitterAsync(stoppingToken);

        using var timer = new PeriodicTimer(_options.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunHealthCheckAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error occurred during health audit execution.");
            }
        }

        logger.LogInformation(
            "Health Audit Background Worker stopped.");
    }

    /// <summary>
    /// Applies randomized startup delay before the first health audit execution.
    /// </summary>
    /// <remarks>
    /// Startup jitter reduces synchronized load spikes caused by multiple instances
    /// performing identical scheduled operations immediately after startup.
    /// </remarks>
    private async Task ApplyStartupJitterAsync(
        CancellationToken cancellationToken)
    {
        if (_options.StartupJitter <= TimeSpan.Zero)
            return;

        var delay = TimeSpan.FromMilliseconds(
            Random.Shared.Next(
                0,
                (int)_options.StartupJitter.TotalMilliseconds));

        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Application shutdown before the first execution.
        }
    }

    /// <summary>
    /// Executes single health collection and persistence cycle.
    /// </summary>
    /// <remarks>
    /// Creates scoped service lifetime because health collectors may depend on
    /// scoped resources such as database sessions or transactional repositories.
    /// </remarks>
    private async Task RunHealthCheckAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var collector = scope.ServiceProvider.GetRequiredService<IHealthCheckCollector>();
        var report = await collector.CollectAndPersistAsync(cancellationToken);

        LogHealthResult(report);
    }

    /// <summary>
    /// Logs health audit result according to current system state.
    /// </summary>
    private void LogHealthResult(SystemHealthReport report)
    {
        var unhealthy = report.Components
            .Where(x => x.Status == HealthStatus.Unhealthy)
            .ToList();

        var degraded = report.Components
            .Where(x => x.Status == HealthStatus.Degraded)
            .ToList();

        switch (report.OverallStatus)
        {
            case HealthStatus.Unhealthy:
                logger.LogCritical(
                    "Health audit failed at {CheckedAt}. " +
                    "Unhealthy: {Unhealthy}, Degraded: {Degraded}. Details: {Details}",
                    report.CheckedAt,
                    unhealthy.Count,
                    degraded.Count,
                    FormatDetails(unhealthy.Concat(degraded)));
                break;

            case HealthStatus.Degraded:
                logger.LogWarning(
                    "Health audit degraded at {CheckedAt}. " +
                    "Degraded: {Degraded}. Details: {Details}",
                    report.CheckedAt,
                    degraded.Count,
                    FormatDetails(degraded));
                break;

            default:
                logger.LogInformation(
                    "Health audit completed at {CheckedAt}. Status: {Status}.",
                    report.CheckedAt,
                    report.OverallStatus);
                break;
        }

        // Full component diagnostics are kept at Debug level to avoid noisy
        // production logs while still allowing deep troubleshooting.
        foreach (var component in report.Components)
        {
            logger.LogDebug(
                "[{Component}] {Status}: {Description}",
                component.Component,
                component.Status,
                component.Description);
        }
    }

    /// <summary>
    /// Formats unhealthy or degraded component details for structured logging.
    /// </summary>
    private static string FormatDetails(
        IEnumerable<ComponentHealth> components)
    {
        return string.Join(
            "; ",
            components.Select(c =>
                $"{c.Component}={c.Status}" +
                (string.IsNullOrEmpty(c.ErrorMessage)
                    ? string.Empty
                    : $" ({c.ErrorMessage})")));
    }
}