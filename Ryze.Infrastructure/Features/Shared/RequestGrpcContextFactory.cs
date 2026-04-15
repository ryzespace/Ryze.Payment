using Grpc.Core;
using Ryze.Domain.Features.WalletBalance.Contexts;
using Ryze.Infrastructure.Features.WalletBalance;

namespace Ryze.Infrastructure.Features.Shared;

/// <summary>
/// Factory for creating <see cref="RequestContext"/> instances from gRPC request data.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Extracts required headers from the <see cref="ServerCallContext"/>.</item>
/// <item>Parses tenant ID, user ID, and correlation ID for context creation.</item>
/// <item>Throws <see cref="Grpc.Core.RpcException"/> if required headers are missing or invalid.</item>
/// <item>Ensures each <see cref="RequestContext"/> is properly initialized for scoped request handling.</item>
/// </list>
/// </remarks>
public static class RequestGrpcContextFactory
{
    /// <summary>
    /// Creates a <see cref="RequestContext"/> from the provided gRPC context.
    /// </summary>
    /// <param name="ctx">The gRPC server call context containing request headers.</param>
    /// <returns>A new <see cref="RequestContext"/> initialized with tenant ID, user ID, and correlation ID.</returns>
    public static RequestContext FromGrpc(ServerCallContext ctx) =>
        RequestContext.Create(
            tenantId: GrpcHeaderParser.RequiredGuid(ctx, "x-tenant-id"),
            userId: GrpcHeaderParser.RequiredGuid(ctx, "x-user-id"),
            idempotencyKey: GrpcHeaderParser.RequiredGuid(ctx, "x-idempotency-key"),
            correlationId: GrpcHeaderParser.RequiredString(ctx, "x-correlation-id")
        );
}
