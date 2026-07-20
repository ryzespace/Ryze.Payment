using ModularityKit.Context.Abstractions;

namespace Ryze.Application.Features.Walllet.Contexts.Getters;

/// <summary>
/// Read context describing a wallet retrieval query.
/// </summary>
/// <remarks>
/// Represents the query intent used to determine which wallet aggregate
/// data and projections should be loaded by the application layer.
///
/// The context allows callers to control optional enrichments such as
/// balance, owners, and limits without coupling the query handler to
/// transport specific request models.
/// </remarks>
public sealed class WalletGetContext : IContext
{
    /// <summary>
    /// Unique identifier of this query context instance.
    /// </summary>
    /// <remarks>
    /// Used for correlation, diagnostics, and tracing during query execution.
    /// </remarks>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Identifier of the wallet being queried.
    /// </summary>
    /// <remarks>
    /// Points to the wallet aggregate for which the read projection is created.
    /// </remarks>
    public required Guid WalletId { get; init; }

    /// <summary>
    /// Determines whether the current wallet balance projection should be loaded.
    /// </summary>
    /// <remarks>
    /// Balance data may be retrieved from a separate projection source rather
    /// than the wallet aggregate itself.
    /// </remarks>
    public bool IncludeBalance { get; init; }

    /// <summary>
    /// Determines whether wallet ownership information should be included.
    /// </summary>
    /// <remarks>
    /// Enables loading of owner-related data when required by the caller.
    /// </remarks>
    public bool IncludeOwners { get; init; }

    /// <summary>
    /// Determines whether wallet limits and restriction data should be included.
    /// </summary>
    public bool IncludeLimits { get; init; }

    /// <summary>
    /// UTC timestamp when this query context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}