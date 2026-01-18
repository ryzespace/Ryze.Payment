using Grpc.Core;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Processors.Interfaces;

/// <summary>
/// Defines read only operations for wallets via gRPC.
/// </summary>
/// <remarks>
/// Provides methods to retrieve  single wallet or list of wallets.
/// </remarks>
public interface IWalletReadOperations
{
    /// <summary>
    /// Retrieves a wallet by its identifier.
    /// </summary>
    /// <param>The request containing wallet identification parameters.</param>
    /// <param>The gRPC server call context.</param>
    /// <returns>A containing wallet data.</returns>
    Task<GetWalletResponse> GetWallet(GetWalletRequest request, ServerCallContext context);

    /// <summary>
    /// Retrieves a list of wallets, optionally filtered.
    /// </summary>
    /// <param>The request specifying filtering or pagination.</param>
    /// <param>The gRPC server call context.</param>
    /// <returns>A containing wallet summaries.</returns>
    Task<ListWalletsResponse> ListWallets(ListWalletsRequest request, ServerCallContext context);
}