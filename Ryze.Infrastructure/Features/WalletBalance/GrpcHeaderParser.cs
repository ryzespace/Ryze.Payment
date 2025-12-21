using Grpc.Core;

namespace Ryze.Infrastructure.Features.WalletBalance;

/// <summary>
/// Utility class for parsing and validating gRPC request headers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides helper methods to extract required headers as <see cref="Guid"/> or <see cref="string"/>.</item>
/// <item>Throws <see cref="RpcException"/> with <see cref="StatusCode.InvalidArgument"/> if a header is missing or invalid.</item>
/// <item>Ensures type-safe extraction of header values for gRPC request processing.</item>
/// </list>
/// </remarks>
internal static class GrpcHeaderParser
{
    /// <summary>
    /// Retrieves required header from the gRPC context and parses it as a <see cref="Guid"/>.
    /// </summary>
    /// <param name="ctx">The gRPC server call context.</param>
    /// <param name="name">The name of the header to retrieve.</param>
    /// <returns>The parsed <see cref="Guid"/> value of the header.</returns>
    /// <exception cref="RpcException">Thrown if the header is missing or the value is not a valid GUID.</exception>
    public static Guid RequiredGuid(ServerCallContext ctx, string name)
    {
        var value = ctx.RequestHeaders.GetValue(name)
                    ?? throw Missing(name);

        return !Guid.TryParse(value, out var guid)
            ? throw Invalid(name, value) : guid;
    }

    public static string RequiredString(ServerCallContext ctx, string name) =>
        ctx.RequestHeaders.GetValue(name)
        ?? throw Missing(name);

    private static RpcException Missing(string name) =>
        new(new Status(StatusCode.InvalidArgument, $"Missing header '{name}'"));

    private static RpcException Invalid(string name, string value) =>
        new(new Status(StatusCode.InvalidArgument, $"Invalid GUID in '{name}': '{value}'"));
}