using Grpc.Core;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Processors.Interfaces;

/// <summary>
/// Defines read-only operations for wallets via gRPC.
/// </summary>
/// <remarks>
/// Provides methods to retrieve  a single wallet or list of wallets.
/// </remarks>
public interface IWalletReadOperations
{
    /// <summary>
    /// Retrieves a wallet by its identifier.
    /// </summary>
    /// <param name="request">The request containing wallet identification parameters.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A response containing wallet data.</returns>
    Task<GetWalletResponse> GetWallet(GetWalletRequest request, ServerCallContext context);

    /// <summary>
    /// Retrieves a list of wallets, optionally filtered.
    /// </summary>
    /// <param name="request">The request specifying filtering or pagination.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A response containing wallet summaries.</returns>
    Task<ListWalletsResponse> ListWallets(ListWalletsRequest request, ServerCallContext context);
}
