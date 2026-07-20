using ModularityKit.Context.Abstractions;
using Ryze.Domain.Features.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context for wallet creation operations.
/// </summary>
/// <remarks>
/// Represents an immutable command intent defining the initial
/// state, ownership, and classification of a wallet at creation time.
/// </remarks>
public sealed class WalletCreationContext : IContext
{
    /// <summary>
    /// Unique identifier of the request context.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Timestamp of context creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Type of the wallet being created.
    /// </summary>
    public required WalletType WalletType { get; init; }

    /// <summary>
    /// Currency assigned to the wallet.
    /// </summary>
    public required Currency CurrencyCode { get; init; }

    /// <summary>
    /// Owners assigned to the wallet at creation time.
    /// </summary>
    public List<WalletOwnerContext> Owners { get; init; } = [];

    /// <summary>
    /// Arbitrary metadata associated with the wallet.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>
    /// Tags used for classification and filtering.
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Returns a concise textual representation of the creation context.
    /// </summary>
    public override string ToString()
        => $"WalletCreationContext {{ Id = {Id}, CurrencyCode = {CurrencyCode} }}";
}