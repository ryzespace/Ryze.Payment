using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RyzeSpace.Payment.Contracts.V1;

namespace Ryze.Infrastructure.Features.Transfers.Processors.Interfaces;

/// <summary>
/// Defines state changing gRPC operations for transfer intents.
/// </summary>
/// <remarks>
/// Exposes commands responsible for creating, confirming, and cancelling
/// transfer intents. Implementations are expected to perform validation,
/// execute the requested operation, and persist the resulting state changes.
/// </remarks>
public interface ITransferIntentWriteOperations
{
    /// <summary>
    /// Creates new transfer intent.
    /// </summary>
    /// <param name="request">Request containing the transfer intent details. </param>
    /// <param name="context">gRPC server call context associated with the current request. </param>
    /// <returns>Response containing the created transfer intent. </returns>
    Task<CreateTransferIntentResponse> CreateTransferIntent(
        CreateTransferIntentRequest request,
        ServerCallContext context);

    /// <summary>
    /// Confirms an existing transfer intent.
    /// </summary>
    /// <remarks>
    /// Confirmation transitions the transfer intent into execution,
    /// allowing the associated transfer workflow to proceed.
    /// </remarks>
    /// <param name="request">Request identifying the transfer intent to confirm. </param>
    /// <param name="context">gRPC server call context associated with the current request. </param>
    /// <returns>Response describing the confirmed transfer intent. </returns>
    Task<ConfirmTransferIntentResponse> ConfirmTransferIntent(
        ConfirmTransferIntentRequest request,
        ServerCallContext context);

    /// <summary>
    /// Cancels an existing transfer intent.
    /// </summary>
    /// <remarks>
    /// Cancels the transfer intent when it has not yet been completed or
    /// otherwise reached a terminal state.
    /// </remarks>
    /// <param name="request">Request identifying the transfer intent to cancel.</param>
    /// <param name="context">gRPC server call context associated with the current request. </param>
    /// <returns>An empty response indicating the cancellation request completed successfully.</returns>
    Task<Empty> CancelTransferIntent(
        CancelTransferIntentRequest request,
        ServerCallContext context);
}