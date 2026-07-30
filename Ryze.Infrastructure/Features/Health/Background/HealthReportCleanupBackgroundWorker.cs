using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ryze.Domain.Features.Health.Repositories;
using Ryze.Infrastructure.Features.Health.Options;

namespace Ryze.Infrastructure.Features.Health.Background;

/// <summary>
/// Background worker responsible for removing expired health reports
/// according to configured retention policy.
/// </summary>
/// <remarks>
/// Periodically removes historical <see cref="Ryze.Domain.Features.Health.ReadModels.SystemHealthReport"/>
/// documents that exceeded the configured retention window.
/// Cleanup is executed immediately after application startup and then continues
/// according to the configured interval. Running an initial cleanup prevents
/// stale health history from accumulating when applications restart frequently
/// and never reach their first scheduled interval.
/// </remarks>
/// <param name="serviceProvider">Service provider used to create scoped repository dependencies. </param>
/// <param name="logger">Logger used for cleanup lifecycle and diagnostic information. </param>
/// <param name="options">Cleanup configuration containing execution interval and retention period. </param>
public sealed class HealthReportCleanupBackgroundWorker(
    IServiceProvider serviceProvider,
    ILogger<HealthReportCleanupBackgroundWorker> logger,
    IOptions<HealthReportCleanupOptions> options) : BackgroundService
{
    private readonly HealthReportCleanupOptions _options = options.Value;

    /// <summary>
    /// Starts the background cleanup execution loop.
    /// </summary>
    /// <remarks>
    /// The first cleanup is executed immediately because <see cref="PeriodicTimer"/>
    /// waits for the configured interval before the first tick.
    /// </remarks>
    /// <param name="stoppingToken">Token used to gracefully stop the worker.</param>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Health Report Cleanup Worker started.");

        await ExecuteCleanupSafelyAsync(stoppingToken);

        using var timer = new PeriodicTimer(_options.Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteCleanupSafelyAsync(stoppingToken);
        }

        logger.LogInformation(
            "Health Report Cleanup Worker stopped.");
    }

    /// <summary>
    /// Executes cleanup operation with background worker level exception handling.
    /// </summary>
    /// <remarks>
    /// Cancellation during shutdown is treated as a normal lifecycle event.
    /// Other exceptions are logged and do not terminate the worker permanently,
    /// allowing future scheduled cleanup executions to continue.
    /// </remarks>
    private async Task ExecuteCleanupSafelyAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await CleanupOldReportsAsync(cancellationToken);
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
                "Error during health report cleanup.");
        }
    }

    /// <summary>
    /// Removes health reports older than configured retention boundary.
    /// </summary>
    /// <remarks>
    /// Creates scoped repository lifetime because persistence implementations
    /// may depend on scoped database sessions.
    /// Deletion itself is performed by the repository, which is responsible for
    /// batching and storage-specific optimization.
    /// </remarks>
    private async Task CleanupOldReportsAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IHealthReportRepository>();

        var cutoff = DateTimeOffset.UtcNow - _options.Retention;
        var deleted = await repository.DeleteOlderThanAsync(
            cutoff, 
            ct: cancellationToken);

        if (deleted == 0)
        {
            logger.LogDebug(
                "No expired health reports found. Cutoff: {Cutoff}.",
                cutoff);

            return;
        }

        logger.LogInformation(
            "Deleted {Count} expired health reports older than {Retention}. Cutoff: {Cutoff}.",
            deleted,
            _options.Retention,
            cutoff);
    }
}