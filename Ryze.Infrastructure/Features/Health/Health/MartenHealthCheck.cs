using Marten;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ryze.Infrastructure.Features.Health.Health;

/// <summary>
/// Health check verifying availability of Marten document storage.
/// </summary>
/// <remarks>
/// Executes lightweight database connectivity probe against the underlying
/// PostgreSQL connection used by Marten.
///
/// A dedicated timeout is applied to prevent database connectivity issues from
/// blocking the entire health evaluation pipeline indefinitely.
/// </remarks>
/// <param name="store">Marten document store used to create database sessions. </param>
public sealed class MartenHealthCheck(
    IDocumentStore store) : IHealthCheck
{
    private static readonly TimeSpan CheckTimeout =
        TimeSpan.FromSeconds(5);

    /// <summary>
    /// Executes Marten storage availability check.
    /// </summary>
    /// <remarks>
    /// The check creates lightweight Marten session, opens the underlying
    /// database connection, and executes a minimal <c>SELECT 1</c> command.
    ///
    /// Timeout failures are reported as degraded instead of unhealthy because
    /// temporary database latency spike does not always indicate a complete
    /// infrastructure failure.
    /// </remarks>
    /// <param name="context">Health check execution context.</param>
    /// <param name="cancellationToken">Token used to cancel the health check execution.</param>
    /// <returns>Health result describing Marten/PostgreSQL availability.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(CheckTimeout);

            await using var session = store.LightweightSession();
            await using var connection = session.Database.CreateConnection();

            await connection.OpenAsync(timeout.Token);
            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT 1";

            await command.ExecuteScalarAsync(timeout.Token);

            return HealthCheckResult.Healthy(
                "Marten/PostgreSQL connection is healthy.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded(
                "Marten/PostgreSQL health check timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Marten/PostgreSQL connection failed.",
                ex);
        }
    }
}