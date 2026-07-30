using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Ryze.Infrastructure.Features.WalletBalance.Options;
using Stripe;

namespace Ryze.Infrastructure.Features.Health.Health;

/// <summary>
/// Health check verifying availability of the Stripe payment provider.
/// </summary>
/// <remarks>
/// Performs lightweight request against the Stripe API by retrieving the
/// current account balance.
/// Missing Stripe configuration is treated as <see cref="HealthStatus.Degraded"/>
/// because the application may still operate without external payment features
/// enabled.
///
/// Stripe API failures are classified as degraded when the provider is reachable
/// but returns an expected API level error. Unexpected failures indicate an
/// infrastructure problem and are reported as unhealthy.
/// </remarks>
/// <param name="options">Configuration containing Stripe API credentials. </param>
public sealed class StripeHealthCheck(
    IOptions<StripeOptions> options) : IHealthCheck
{
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Executes Stripe availability verification.
    /// </summary>
    /// <remarks>
    /// The external request is protected by an internal timeout to prevent
    /// slow third party dependency from blocking the health evaluation loop.
    /// </remarks>
    /// <param name="context">Health check execution context provided by the health check framework. </param>
    /// <param name="cancellationToken">Token used to cancel the health check operation.</param>
    /// <returns>Health result describing Stripe availability.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            return HealthCheckResult.Degraded(
                "Stripe API key not configured.");
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(CheckTimeout);

            var client = new StripeClient(options.Value.ApiKey);
            var service = new BalanceService(client);

            await service.GetAsync(cancellationToken: cts.Token);

            return HealthCheckResult.Healthy(
                "Stripe API is reachable.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded(
                "Stripe API health check timed out.");
        }
        catch (StripeException ex)
        {
            return HealthCheckResult.Degraded(
                $"Stripe API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Stripe API check failed.",
                ex);
        }
    }
}