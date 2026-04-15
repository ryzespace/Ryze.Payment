using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts.Getters;

/// <summary>
/// Read context for wallet retrieval operations.
/// </summary>
/// <remarks>
/// Represents an immutable query intent describing which wallet
/// data projections should be loaded by the application layer.
/// </remarks>
public sealed class WalletGetContext : IContext
{
    /// <summary>
    /// Unique identifier of the request context.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet being queried.
    /// </summary>
    public required Guid WalletId { get; init; }

    /// <summary>
    /// Indicates whether wallet balance should be included.
    /// </summary>
    public bool IncludeBalance { get; init; }

    /// <summary>
    /// Indicates whether wallet owners should be included.
    /// </summary>
    public bool IncludeOwners { get; init; }

    /// <summary>
    /// Indicates whether wallet limits should be included.
    /// </summary>
    public bool IncludeLimits { get; init; }

    /// <summary>
    /// Timestamp of context creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
