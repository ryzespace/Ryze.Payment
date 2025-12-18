using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ryze.Infrastructure.Features.WalletBalance.Processors.Interfaces;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.WalletBalance.Processors.Processor;

/// <summary>
/// gRPC service implementation for managing wallet balances.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="WalletBalances.WalletBalancesBase"/> generated from the gRPC contract.</item>
/// <item>Delegates all write operations to an <see cref="IWalletBalanceWriteOperations"/> instance.</item>
/// <item>Encapsulates the business logic for top-up operations while exposing a gRPC interface.</item>
/// </list>
/// </remarks>
public class WalletBalanceManagementProcessor(
    IWalletBalanceWriteOperations writeOperations) : WalletBalances.WalletBalancesBase
{
    /// <inheritdoc />
    public override Task<Empty> TopUpBalance(WalletTopUpBalanceRequest request, ServerCallContext context)
        => writeOperations.TopUpBalance(request, context);
}