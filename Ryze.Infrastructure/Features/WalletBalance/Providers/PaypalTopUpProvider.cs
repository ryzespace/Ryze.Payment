using ModularityKit.Context.Abstractions;
using Ryze.Domain.Features.WalletBalance;
using Ryze.Domain.Features.WalletBalance.Contexts;
using Ryze.Domain.Features.WalletBalance.Providers;

namespace Ryze.Infrastructure.Features.WalletBalance.Providers;

/// <summary>
/// Implementation of <see cref="ITopUpProvider"/> for PayPal payment provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles wallet top-up operations using PayPal.</item>
/// <item>Exposes <see cref="ProviderType"/> as <see cref="PaymentProvider.PayPal"/>.</item>
/// </list>
/// </remarks>
public class PaypalTopUpProvider(IContextAccessor<RequestContext> request, IContextAccessor<WalletContext> wallet) : ITopUpProvider
{
    /// <inheritdoc />
    public PaymentProvider ProviderType => PaymentProvider.PayPal;

    /// <inheritdoc />
    public Task<double> TopUp(decimal amount)
    {
        var requestCtx = request.RequireCurrent();
        var walletCtx = wallet.RequireCurrent();
        requestCtx.WithRequestType("TopUp");
        
        Console.WriteLine(
            $"Top-up request {requestCtx.CorrelationId} for Tenant {requestCtx.TenantId}, User {requestCtx.UserId}");
        
        walletCtx.SetLastTopUp(amount, ProviderType);
        Console.WriteLine($"Wallet {walletCtx.WalletId} topped up {walletCtx.LastTopUpAmount} via {walletCtx.LastTopUpProvider} at {walletCtx.LastTopUpAt} by PayPal");
      
        Console.WriteLine(
            $"Top-up {amount} done via {ProviderType} ({requestCtx.RequestType ?? "unknown type"})");

        if (amount > 1000)
        {
            requestCtx.EnableFeature("HighValueTopUp");
            Console.WriteLine(
                $"Top-up Limit {amount})");
        }

        return Task.FromResult((double)amount);
    }
}