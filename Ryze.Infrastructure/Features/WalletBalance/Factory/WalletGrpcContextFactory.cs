using Grpc.Core;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Infrastructure.Features.WalletBalance.Factory;

/// <summary>
/// Factory for creating <see cref="WalletContext"/> instances from gRPC request data.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Extracts required headers from the <see cref="ServerCallContext"/>.</item>
/// <item>Parses the wallet identifier and idempotency key for context creation.</item>
/// <item>Throws <see cref="RpcException"/> if required headers are missing or invalid.</item>
/// <item>Ensures each <see cref="WalletContext"/> is initialized with a unique request scope for safe operations.</item>
/// </list>
/// </remarks>
public static class WalletGrpcContextFactory
{
    /// <summary>
    /// Creates <see cref="WalletContext"/> from the provided gRPC context and wallet ID.
    /// </summary>
    /// <param name="ctx">The gRPC server call context containing request headers.</param>
    /// <param name="walletId">The wallet identifier as a string.</param>
    /// <returns>A new <see cref="WalletContext"/> initialized with the wallet ID and idempotency key.</returns>
    /// <exception cref="RpcException">Thrown if the "x-idempotency-key" header is missing.</exception>
    public static WalletContext FromGrpc(
        ServerCallContext ctx,
        string walletId)
    {
        var key = ctx.RequestHeaders.GetValue("x-idempotency-key")
                  ?? throw new RpcException(
                      new Status(StatusCode.InvalidArgument, "Missing x-idempotency-key"));

        return WalletContext.Create(
            walletId: Guid.Parse(walletId),
            idempotencyKey: key
        );
    }
}