using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context for wallet suspension operations.
/// </summary>
/// <remarks>
/// Represents command intent requesting transition of a wallet
/// from an active state to a suspended state.
/// </remarks>
public sealed class WalletSuspendContext : IContext
{
    /// <summary>
    /// Unique identifier of the request context.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet being suspended.
    /// </summary>
    public Guid WalletId { get; init; }

    /// <summary>
    /// Reason for wallet suspension.
    /// </summary>
    public string Reason { get; init; } = null!;

    /// <summary>
    /// Timestamp of context creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}