using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Processors.Interfaces;

/// <summary>
/// Defines write operations for wallet management via gRPC.
/// </summary>
/// <remarks>
/// Provides asynchronous methods for creating, suspending, and reactivating wallets.
/// Implementations handle the business logic of Wallet commands.
/// </remarks>
public interface IWalletWriteOperations
{
    /// <summary>
    /// Creates new wallet.
    /// </summary>
    /// <param>Request containing owners, initial balance, wallet type, and currency.</param>
    /// <param>gRPC server call context.</param>
    /// <returns>Response with wallet ID, creation timestamp, and status.</returns>
    Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request, ServerCallContext context);

    /// <summary>
    /// Suspends an existing wallet.
    /// </summary>
    /// <param>Request containing the wallet ID, reason, and optional suspend-until timestamp.</param>
    /// <param>gRPC server call context.</param>
    /// <returns>An empty response on success.</returns>
    Task<Empty> SuspendWallet(SuspendWalletRequest request, ServerCallContext context);

    /// <summary>
    /// Reactivates a suspended wallet.
    /// </summary>
    /// <param>Request containing the wallet ID and reason for reactivation.</param>
    /// <param>gRPC server call context.</param>
    /// <returns>An empty response on success.</returns>
    Task<Empty> ReactivateWallet(ReactivateWalletRequest request, ServerCallContext context);
}