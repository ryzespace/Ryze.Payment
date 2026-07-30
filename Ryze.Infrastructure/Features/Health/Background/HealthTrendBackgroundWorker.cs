using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ryze.Domain.Features.Health.Enum;
using Ryze.Domain.Features.Health.Repositories;
using Ryze.Infrastructure.Features.Health.Options;

namespace Ryze.Infrastructure.Features.Health.Background;

/// <summary>
/// Background worker responsible for calculating historical health trends
/// from persisted system health reports.
/// </summary>
/// <remarks>
/// Periodically analyzes health report history across predefined time windows
/// and produces availability, status distribution, and execution duration metrics.
///
/// Trend calculation intentionally loads the widest required window once and
/// derives smaller windows in memory. This avoids multiple database round trips
/// per execution while keeping all shorter periods consistent with the same
/// snapshot of health history.
/// </remarks>
/// <param name="serviceProvider">Service provider used to create scoped repository dependencies. </param>
/// <param name="logger">Logger used for trend calculation diagnostics. </param>
/// <param name="options">Configuration controlling execution interval, jitter, and calculation limits. </param>
public sealed class HealthTrendBackgroundWorker(
    IServiceProvider serviceProvider,
    ILogger<HealthTrendBackgroundWorker> logger,
    IOptions<HealthTrendOptions> options) : BackgroundService
{
    private readonly HealthTrendOptions _options = options.Value;

    /// <summary>
    /// Predefined historical analysis windows.
    /// </summary>
    /// <remarks>
    /// Windows are ordered from shortest to longest. The largest window is used
    /// as the source query range; smaller windows are calculated from the same
    /// loaded dataset.
    /// </remarks>
    private static readonly (int Hours, string Label)[] Windows =
    [
        (1, "1h"),
        (24, "24h"),
        (168, "7d")
    ];

    /// <summary>
    /// Starts periodic health trend calculation.
    /// </summary>
    /// <remarks>
    /// Startup jitter is applied before the first execution to avoid synchronized
    /// execution across multiple application instances.
    /// </remarks>
    /// <param name="stoppingToken">Token used to gracefully stop the worker. </param>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Health Trend Worker started.");

        await ApplyStartupJitterAsync(stoppingToken);

        using var timer = new PeriodicTimer(_options.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteTrendCalculationSafelyAsync(stoppingToken);
        }

        logger.LogInformation(
            "Health Trend Worker stopped.");
    }

    /// <summary>
    /// Applies randomized startup delay before the first trend calculation.
    /// </summary>
    /// <remarks>
    /// Prevents multiple service instances from querying and processing health
    /// history at exactly the same moment after deployment or restart.
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
            // Expected during application shutdown.
        }
    }

    /// <summary>
    /// Executes trend calculation with worker-level exception isolation.
    /// </summary>
    private async Task ExecuteTrendCalculationSafelyAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await ComputeTrendAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Expected during application shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error during health trend computation.");
        }
    }

    /// <summary>
    /// Calculates health availability trends for configured historical windows.
    /// </summary>
    /// <remarks>
    /// Only the widest configured time window is loaded from storage.
    /// This reduces repository calls from one per window to a single query per
    /// execution cycle.
    /// </remarks>
    private async Task ComputeTrendAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IHealthReportRepository>();

        var now = DateTimeOffset.UtcNow;

        var widestWindow = Windows.MaxBy(x => x.Hours);
        var widestFrom = now.AddHours(-widestWindow.Hours);

        var reports = await repository.GetByPeriodAsync(
            widestFrom,
            now,
            _options.MaxReportsPerWindow,
            cancellationToken);

        if (reports.Count == _options.MaxReportsPerWindow)
        {
            logger.LogWarning(
                "Health trend query reached report limit {Limit}. " +
                "The {Window} trend may contain only the newest available subset.",
                _options.MaxReportsPerWindow,
                widestWindow.Label);
        }

        foreach (var (hours, label) in Windows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CalculateWindowTrend(
                reports,
                now.AddHours(-hours),
                label);
        }
    }

    /// <summary>
    /// Calculates and logs trend statistics for a single time window.
    /// </summary>
    private void CalculateWindowTrend(
        IReadOnlyList<Ryze.Domain.Features.Health.ReadModels.SystemHealthReport> reports,
        DateTimeOffset from,
        string label)
    {
        var windowReports = reports
            .Where(r => r.CheckedAt >= from)
            .ToList();

        if (windowReports.Count == 0)
        {
            logger.LogDebug(
                "No health reports found for window {Label}.",
                label);

            return;
        }

        var total = windowReports.Count;

        var healthy = windowReports.Count(x =>
            x.OverallStatus == HealthStatus.Healthy);

        var degraded = windowReports.Count(x => 
            x.OverallStatus == HealthStatus.Degraded);

        var unhealthy = windowReports.Count(x =>
            x.OverallStatus == HealthStatus.Unhealthy);

        var availability = healthy * 100.0 / total;

        var averageDuration = windowReports.Average(
            x => x.TotalDuration.TotalMilliseconds);

        logger.LogInformation(
            "Health trend [{Label}] Total={Total}, Healthy={Healthy}, " +
            "Degraded={Degraded}, Unhealthy={Unhealthy}, " +
            "Availability={Availability:F1}%, AvgDuration={AverageDuration:F0}ms",
            label,
            total,
            healthy,
            degraded,
            unhealthy,
            availability,
            averageDuration);

        if (availability < _options.AvailabilityThresholdPercent)
        {
            logger.LogWarning(
                "Availability below threshold for {Label}: {Availability:F1}% " +
                "(threshold: {Threshold:F1}%).",
                label,
                availability,
                _options.AvailabilityThresholdPercent);
        }
    }
}