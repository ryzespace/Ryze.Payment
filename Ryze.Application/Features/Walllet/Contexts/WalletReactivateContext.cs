using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context for wallet reactivation operations.
/// </summary>
/// <remarks>
/// Represents a command intent requesting transition of a wallet
/// from a suspended state back to an active state.
/// </remarks>
public sealed class WalletReactivateContext : IContext
{
    /// <summary>
    /// Unique identifier of the request context.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet being reactivated.
    /// </summary>
    public Guid WalletId { get; init; }

    /// <summary>
    /// Reason for wallet reactivation.
    /// </summary>
    public string Reason { get; init; }

    /// <summary>
    /// Timestamp of context creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}