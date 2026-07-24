using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Ledger.Interfaces;
using Ryze.Application.Features.Ledger.Verification;

namespace Ryze.Infrastructure.Features.Ledger.Background;

/// <summary>
/// Background worker that periodically performs full ledger integrity audits.
/// </summary>
/// <remarks>
/// Executes complete ledger integrity verification at fixed interval,
/// including balance reconciliation and account chain validation. Audit
/// failures are logged as critical events, while successful audits are
/// recorded as informational messages.
/// </remarks>
public sealed class LedgerAuditBackgroundWorker(
    IServiceProvider serviceProvider,
    ILedgerIntegrityTracker integrityTracker,
    ILogger<LedgerAuditBackgroundWorker> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    /// <summary>
    /// Executes the periodic ledger integrity audit loop.
    /// </summary>
    /// <param name="stoppingToken">Token that signals cancellation of the background worker.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Ledger Audit Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAuditAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during ledger audit.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    /// <summary>
    /// Creates scoped integrity guard and executes full ledger integrity audit.
    /// </summary>
    /// <param name="ct">Token that can be used to cancel the audit operation.</param>
    private async Task RunAuditAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var integrityGuard = scope.ServiceProvider.GetRequiredService<LedgerIntegrityGuard>();

        var report = await integrityGuard.VerifyFullIntegrityAsync(ct);
        integrityTracker.UpdateReport(report);

        if (!report.IsHealthy)
        {
            logger.LogCritical(
                "Ledger Integrity Audit FAILED at {Time}. Issues: {Issues}",
                report.CheckedAt,
                string.Join("; ", report.Issues));
        }
        else
        {
            logger.LogInformation(
                "Ledger Integrity Audit PASSED at {Time}.",
                report.CheckedAt);
        }
    }
}