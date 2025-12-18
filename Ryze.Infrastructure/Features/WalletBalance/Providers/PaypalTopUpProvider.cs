using Ryze.Domain.Features.WalletBalance;

namespace Ryze.Infrastructure.Features.WalletBalance.Providers;

/// <summary>
/// Implementation of <see cref="ITopUpProvider"/> for PayPal payment provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles wallet top-up operations using PayPal.</item>
/// <item>Exposes <see cref="ProviderType"/> as <see cref="PaymentProvider.PayPal"/>.</item>
/// <item>Can be used by higher-level services or processors to delegate PayPal-based top-ups.</item>
/// </list>
/// </remarks>
public class PaypalTopUpProvider : ITopUpProvider
{
    /// <inheritdoc />
    public PaymentProvider ProviderType => PaymentProvider.PayPal;
  
    /// <inheritdoc />
    public Task<double> TopUp(string walletId, double amount)
    {
        // logic PayPal SDK
        Console.WriteLine($"Top-up {amount} for {walletId} by PayPal");
        return Task.FromResult(amount);
    }
}