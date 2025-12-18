using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.WalletBalance.Processors.Interfaces;

/// <summary>
/// Defines write operations for managing wallet balances.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides a contract for gRPC-based operations related to wallet balance updates.</item>
/// <item>Designed to be implemented by processors handling the business logic of wallet transactions.</item>
/// </list>
/// </remarks>
public interface IWalletBalanceWriteOperations
{
    /// <summary>
    /// Tops up the balance of a wallet based on the provided request.
    /// </summary>
    /// <param name="request">A <see cref="WalletTopUpBalanceRequest"/> containing the wallet identifier and amount to top up.</param>
    /// <param name="context">The <see cref="ServerCallContext"/> provided by the gRPC runtime.</param>
    /// <returns>An <see cref="Empty"/> response indicating completion of the operation.</returns>
    Task<Empty> TopUpBalance(WalletTopUpBalanceRequest request, ServerCallContext context);
}