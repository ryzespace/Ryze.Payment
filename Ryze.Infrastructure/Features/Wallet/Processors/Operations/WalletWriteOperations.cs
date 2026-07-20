using Contracts.Shared.Extensions;
using Contracts.Shared.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;
using Ryze.Infrastructure.Features.Wallet.Mapping;
using Ryze.Infrastructure.Features.Wallet.Processors.Interfaces;
using Ryze.Infrastructure.Shared;
using RyzeSpace.Wallet.Contracts.V1;
using Wolverine;

namespace Ryze.Infrastructure.Features.Wallet.Processors.Operations;

/// <summary>
/// Implements write operations for wallets via gRPC.
/// </summary>
/// <remarks>
/// Provides methods for creating, suspending, and reactivating wallets.
/// Integrates with <see>
///     <cref>IMessageBus</cref>
/// </see>
/// and domain contexts.
/// </remarks>
public class WalletWriteOperations(IMessageBus bus, ILogger<WalletWriteOperations> logger) : IWalletWriteOperations
{
    /// <summary>
    /// Creates a new wallet.
    /// </summary>
    /// <param name="request">The gRPC request containing wallet creation parameters.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>A response with wallet ID, status, and creation timestamp.</returns>
    public async Task<CreateWalletResponse> CreateWallet(CreateWalletRequest request, ServerCallContext context)
    {
        logger.LogGrpcRequest("CreateWallet", request);

        var requestCtx = RequestGrpcContextFactory.FromGrpc(context)
            .WithRequestType("CreateWallet");

        var walletCtx = request.MapTo<CreateWalletRequest, WalletCreationContext>(logger);
        var command = new CreateWalletCommand(requestCtx, walletCtx);

        var responseDto = await bus.InvokeAsync<CreateWalletResponseDto>(command);

        logger.LogOperationCompleted(
            "CreateWallet",
            new
            {
                responseDto.WalletId,
                responseDto.Status,
                responseDto.CreatedAt
            }
        );
        return responseDto.MapTo<CreateWalletResponseDto, CreateWalletResponse>(logger);
    }

    /// <summary>
    /// Suspends an existing wallet.
    /// </summary>
    /// <param name="request">The gRPC request containing wallet ID, reason, and optional suspend-until timestamp.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>An empty response on success.</returns>
    public async Task<Empty> SuspendWallet(SuspendWalletRequest request, ServerCallContext context)
    {
        logger.LogGrpcRequest("SuspendWallet", request);

        var requestCtx = RequestGrpcContextFactory.FromGrpc(context)
            .WithRequestType("SuspendWallet");

        var suspendCtx = WalletStateChangeMapping.ToContext(request);
        var command = new SuspendWalletCommand(requestCtx, suspendCtx);

        await bus.InvokeAsync(command);

        logger.LogOperationCompleted(
            "SuspendWallet",
            requestCtx.Id
        );
        return new Empty();
    }

    /// <summary>
    /// Reactivates suspended wallet.
    /// </summary>
    /// <param name="request">The gRPC request containing wallet ID and reason for reactivation.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>An empty response on success.</returns>
    public async Task<Empty> ReactivateWallet(ReactivateWalletRequest request, ServerCallContext context)
    {
        logger.LogGrpcRequest("ReactivateWallet", request);

        var requestCtx = RequestGrpcContextFactory.FromGrpc(context)
            .WithRequestType("ReactivateWallet");

        var reactivateCtx = WalletStateChangeMapping.ToContext(request);
        var command = new ReactivateWalletCommand(requestCtx, reactivateCtx);

        await bus.InvokeAsync(command);
        logger.LogOperationCompleted(
            "ReactivateWallet",
            requestCtx.Id
        );
        return new Empty();
    }
}
