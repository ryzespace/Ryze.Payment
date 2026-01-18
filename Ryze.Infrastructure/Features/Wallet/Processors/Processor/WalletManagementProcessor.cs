using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ryze.Infrastructure.Features.Wallet.Processors.Interfaces;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Processors.Processor;

/// <summary>
/// gRPC service adapter for Wallet management.
/// </summary>
/// <remarks>
/// Delegates gRPC calls to <see><cref>IWalletWriteOperations</cref></see>
/// and <see><cref>IWalletReadOperations</cref></see>
/// . Acts purely as a transport layer
/// between gRPC and the application/domain layer.
/// </remarks>
public sealed class WalletManagementProcessor(
    IWalletWriteOperations writeOperations,
    IWalletReadOperations readOperations
) : Wallets.WalletsBase
{
    /// <summary>Creates a new wallet.</summary>
    public override Task<CreateWalletResponse> CreateWallet(
        CreateWalletRequest request,
        ServerCallContext context)
        => writeOperations.CreateWallet(request, context);

    /// <summary>Suspends an existing wallet.</summary>
    public override Task<Empty> SuspendWallet(
        SuspendWalletRequest request,
        ServerCallContext context)
        => writeOperations.SuspendWallet(request, context);

    /// <summary>Reactivates a suspended wallet.</summary>
    public override Task<Empty> ReactivateWallet(
        ReactivateWalletRequest request,
        ServerCallContext context)
        => writeOperations.ReactivateWallet(request, context);

    /// <summary>Retrieves a wallet by its identifier.</summary>
    public override Task<GetWalletResponse> GetWallet(
        GetWalletRequest request,
        ServerCallContext context)
        => readOperations.GetWallet(request, context);

    /// <summary>Retrieves a list of wallets.</summary>
    public override Task<ListWalletsResponse> ListWallets(
        ListWalletsRequest request,
        ServerCallContext context)
        => readOperations.ListWallets(request, context);
}