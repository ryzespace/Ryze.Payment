using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.WalletBalance.UseCase.Commands.Requests;
using Ryze.Domain.Features.WalletBalance;
using Ryze.Domain.Features.WalletBalance.Contexts;
using Ryze.Domain.Features.WalletBalance.Providers;
using Ryze.Infrastructure.Features.WalletBalance.Factory;
using Ryze.Infrastructure.Features.WalletBalance.Processors.Interfaces;
using RyzeSpace.Wallet.Contracts.V1;
using Wolverine;

namespace Ryze.Infrastructure.Features.WalletBalance.Processors.Operations;

/// <summary>
/// Implementation of <see cref="IWalletBalanceWriteOperations"/> that handles wallet balance write operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Delegates top-up operations to registered <see cref="ITopUpProvider"/> instances based on <see cref="PaymentProvider"/>.</item>
/// <item>Maps gRPC request <see cref="PaymentProviders"/> to internal <see cref="PaymentProvider"/> enum.</item>
/// <item>Logs top-up operations and ensures type-safe provider selection.</item>
/// <item>Executes operations within <see cref="RequestContext"/> and <see cref="WalletContext"/> using <see cref="IContextManager{T}"/>.</item>
/// <item>Publishes <see cref="WalletTopUpCommand"/> to the <see cref="IMessageBus"/> for further processing.</item>
/// </list>
/// </remarks>
public class WalletWriteOperations(
    IContextManager<RequestContext> requestContext,
    IContextManager<WalletContext> walletContext,
    IMessageBus bus)
    : IWalletBalanceWriteOperations
{
    /// <inheritdoc />
    public async Task<Empty> TopUpBalance(WalletTopUpBalanceRequest request, ServerCallContext context)
    {
        var requestCtx = RequestGrpcContextFactory.FromGrpc(context);
        var walletCtx = WalletGrpcContextFactory.FromGrpc(
            context,
            request.WalletId);
        
        await requestContext.ExecuteInContext(requestCtx, async () =>
        {
            await walletContext.ExecuteInContext(walletCtx, async () =>
            {
                await bus.InvokeAsync(new WalletTopUpCommand(
                    (decimal)request.Amount,
                    (PaymentProvider)request.Provider
                ));
            });
        });
        
        return new Empty();
    }
}