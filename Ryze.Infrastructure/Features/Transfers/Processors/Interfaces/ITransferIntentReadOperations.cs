using Grpc.Core;
using RyzeSpace.Payment.Contracts.V1;

namespace Ryze.Infrastructure.Features.Transfers.Processors.Interfaces;

/// <summary>
/// Defines read only gRPC operations for querying transfer intents.
/// </summary>
/// <remarks>
/// Exposes retrieval operations for individual transfer intents and paginated
/// collections. Implementations are intended to be consumed by the transfer
/// gRPC service and must not modify transfer state.
/// </remarks>
public interface ITransferIntentReadOperations
{
    /// <summary>
    /// Retrieves single transfer intent.
    /// </summary>
    /// <param name="request">request identifying the transfer intent to retrieve. </param>
    /// <param name="context">gRPC server call context associated with the current request.</param>
    /// <returns>Response containing the requested transfer intent.</returns>
    Task<GetTransferIntentResponse> GetTransferIntent(
        GetTransferIntentRequest request,
        ServerCallContext context);

    /// <summary>
    /// Retrieves transfer intents matching the requested criteria.
    /// </summary>
    /// <remarks>
    /// Supports listing transfer intents for reporting, administrative
    /// operations, and client-side browsing.
    /// </remarks>
    /// <param name="request">Request containing filtering and pagination parameters.</param>
    /// <param name="context">gRPC server call context associated with the current request.</param>
    /// <returns>Response containing the matching transfer intents.</returns>
    Task<ListTransferIntentsResponse> ListTransferIntents(
        ListTransferIntentsRequest request,
        ServerCallContext context);
}