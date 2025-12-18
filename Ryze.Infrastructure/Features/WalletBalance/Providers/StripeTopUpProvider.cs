using Ryze.Domain.Features.WalletBalance;

namespace Ryze.Infrastructure.Features.WalletBalance.Providers;

/// <summary>
/// Implementation of <see cref="ITopUpProvider"/> for Stripe payment provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles wallet top-up operations using Stripe.</item>
/// <item>Exposes <see cref="ProviderType"/> as <see cref="PaymentProvider.Stripe"/>.</item>
/// <item>Can be used by higher-level services or processors to delegate Stripe-based top-ups.</item>
/// </list>
/// </remarks>
public class StripeTopUpProvider : ITopUpProvider
{
    /// <inheritdoc />
    public PaymentProvider ProviderType => PaymentProvider.Stripe;
    
    /// <inheritdoc />
    public Task<double> TopUp(string walletId, double amount)
    {
        // logic Stripe SDK
        Console.WriteLine($"Top-up {amount} for {walletId} by Stripe");
        return Task.FromResult(amount);
    }
}