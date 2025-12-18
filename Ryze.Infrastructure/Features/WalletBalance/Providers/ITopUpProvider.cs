using Ryze.Domain.Features.WalletBalance;

namespace Ryze.Infrastructure.Features.WalletBalance.Providers;

/// <summary>
/// Defines a contract for services that can top up wallet balances via specific payment providers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Each implementation corresponds to a specific <see cref="PaymentProvider"/>.</item>
/// <item>Responsible for executing the top-up transaction and returning the updated balance.</item>
/// <item>Can be used by higher-level processors to delegate top-up operations to the appropriate provider.</item>
/// </list>
/// </remarks>
public interface ITopUpProvider
{
    /// <summary>
    /// Gets the type of payment provider implemented by this service.
    /// </summary>
    PaymentProvider ProviderType { get; }

    /// <summary>
    /// Tops up the wallet with the specified amount.
    /// </summary>
    /// <param name="walletId">The identifier of the wallet to top up.</param>
    /// <param name="amount">The amount to add to the wallet balance.</param>
    /// <returns>The new balance of the wallet after the top-up operation.</returns>
    Task<double> TopUp(string walletId, double amount);
}