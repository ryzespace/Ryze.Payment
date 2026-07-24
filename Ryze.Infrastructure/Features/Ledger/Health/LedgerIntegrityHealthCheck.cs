using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ryze.Application.Features.Ledger.Interfaces;

namespace Ryze.Infrastructure.Features.Ledger.Health;

/// <summary>
/// Health check that verifies the integrity and balance of the ledger.
/// </summary>
/// <remarks>
/// Executes full ledger integrity verification and reports the result
/// through health check infrastructure. The check is
/// considered healthy when all ledger integrity validations pass and
/// unhealthy when inconsistencies or verification errors are detected.
/// </remarks>
public sealed class LedgerIntegrityHealthCheck(
    ILedgerIntegrityTracker integrityTracker) : IHealthCheck
{
    /// <summary>
    /// Performs full ledger integrity check and returns the corresponding health status.
    /// </summary>
    /// <param name="context">The context containing information about the health check execution.</param>
    /// <param name="cancellationToken">Token that can be used to cancel the health check operation.</param>
    /// <returns>
    /// A task containing a <see cref="HealthCheckResult"/> indicating whether
    /// the ledger is healthy, contains integrity issues, or could not be verified.
    /// </returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var report = integrityTracker.GetLatestReport();

        if (report == null)
        {
            return Task.FromResult(HealthCheckResult.Degraded("Ledger integrity audit is pending first execution."));
        }

        if (report.IsHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Ledger is balanced and healthy. Verified {report.AccountsChecked} accounts and {report.TotalEntriesVerified} entries.",
                data: new Dictionary<string, object>
                {
                    { "AccountsChecked", report.AccountsChecked },
                    { "TotalEntriesVerified", report.TotalEntriesVerified },
                    { "CheckedAt", report.CheckedAt }
                }));
        }

        var data = report.Issues.ToDictionary(i => i, object (i) => i);
        data.Add("AccountsChecked", report.AccountsChecked);
        data.Add("TotalEntriesVerified", report.TotalEntriesVerified);
        data.Add("CheckedAt", report.CheckedAt);

        return Task.FromResult(HealthCheckResult.Unhealthy(
            "Ledger integrity issues detected.",
            data: data));
    }
}