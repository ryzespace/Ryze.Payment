using Contracts.Shared.Extensions;
using Contracts.Shared.Grpc;
using Grpc.Core;
using Mapster;
using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.UseCase.Queries.Requests;
using Ryze.Infrastructure.Features.Wallet.Processors.Interfaces;
using Ryze.Infrastructure.Shared;
using RyzeSpace.Wallet.Contracts.V1;
using Wolverine;

using ProtoWallet = RyzeSpace.Wallet.Contracts.V1.Wallet;

namespace Ryze.Infrastructure.Features.Wallet.Processors.Operations;

/// <summary>
/// Implements read-only gRPC operations for Wallets.
/// </summary>
/// <remarks>
/// Handles retrieval of a single wallet or list of wallets using domain contexts
/// and integrates with the message bus and logging infrastructure.
/// </remarks>
public class WalletRoadOperations(IMessageBus bus, ILogger<WalletRoadOperations> logger) : IWalletReadOperations
{
    /// <summary>
    /// Retrieves wallet by its identifier.
    /// </summary>
    /// <param name="request">The gRPC request containing wallet identification.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A response containing the wallet data.</returns>
    public async Task<GetWalletResponse> GetWallet(GetWalletRequest request, ServerCallContext context)
    {
        logger.LogGrpcRequest("GetWallet", request);

        var requestCtx = RequestGrpcContextFactory.FromGrpc(context)
            .WithRequestType("GetWallet");

        var walletCtx = request.MapTo<GetWalletRequest, WalletGetContext>(logger);
        var query = new GetWalletQueries(requestCtx, walletCtx);

        var responseDto = await bus.InvokeAsync<GetWalletResponseDto>(query);

        logger.LogOperationCompleted("GetWallet", new { responseDto });

        var walletProtos = responseDto.Wallet.Adapt<ProtoWallet>();
        return new GetWalletResponse { Wallet = walletProtos };
    }

    /// <summary>
    /// Retrieves list of wallets.
    /// </summary>
    /// <param name="request">The gRPC request containing filtering or pagination parameters.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A response containing wallet summaries.</returns>
    public async Task<ListWalletsResponse> ListWallets(ListWalletsRequest request, ServerCallContext context)
    {
        logger.LogGrpcRequest("ListWallets", request);

        var requestCtx = RequestGrpcContextFactory.FromGrpc(context)
            .WithRequestType("ListWallets");

        var walletCtx = request.MapTo<ListWalletsRequest, WalletListContext>(logger);
        var query = new ListWalletsQueries(requestCtx, walletCtx);

        var responseDto = await bus.InvokeAsync<ListWalletsResponseDto>(query);
        return responseDto.MapTo<ListWalletsResponseDto, ListWalletsResponse>(logger);
    }
}
