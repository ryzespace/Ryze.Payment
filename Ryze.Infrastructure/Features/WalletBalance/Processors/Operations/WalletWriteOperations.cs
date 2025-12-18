using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ryze.Domain.Features.WalletBalance;
using Ryze.Infrastructure.Features.WalletBalance.Processors.Interfaces;
using Ryze.Infrastructure.Features.WalletBalance.Providers;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.WalletBalance.Processors.Operations;

/// <summary>
/// Implementation of <see cref="IWalletBalanceWriteOperations"/> that handles wallet balance write operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Delegates top-up operations to registered <see cref="ITopUpProvider"/> instances based on <see cref="PaymentProvider"/>.</item>
/// <item>Maps gRPC request <see cref="PaymentProviders"/> to internal <see cref="PaymentProvider"/> enum.</item>
/// <item>Logs top-up operations and ensures type-safe provider selection.</item>
/// </list>
/// </remarks>
public class WalletWriteOperations : IWalletBalanceWriteOperations
{
    private readonly Dictionary<PaymentProvider, ITopUpProvider> _providers;

    public WalletWriteOperations(IEnumerable<ITopUpProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderType);
    }
    
    /// <inheritdoc />
    public async Task<Empty> TopUpBalance(WalletTopUpBalanceRequest request, ServerCallContext context)
    {
        var providerType = MapProvider(request.Provider);

        if (!_providers.TryGetValue(providerType, out var provider))
            throw new InvalidOperationException("Nieznany provider top-up");

        var added = await provider.TopUp(request.WalletId, request.Amount);

        Console.WriteLine($"Wallet {request.WalletId} topped up {added} via {provider.ProviderType}");
        return new Empty();
    }
    
    private static PaymentProvider MapProvider(PaymentProviders protoProvider) => protoProvider switch
    {
        PaymentProviders.Stripe => PaymentProvider.Stripe,
        PaymentProviders.PayPal => PaymentProvider.PayPal,
        _ => PaymentProvider.Unknown
    };
}