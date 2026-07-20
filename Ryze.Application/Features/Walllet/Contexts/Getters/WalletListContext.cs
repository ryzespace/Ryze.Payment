using ModularityKit.Context.Abstractions;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Contexts.Getters;

/// <summary>
/// Read context describing a wallet listing query.
/// </summary>
/// <remarks>
/// Represents the query intent used to retrieve a filtered and paginated
/// collection of wallets.
///
/// The context contains cursor-based pagination settings, optional domain
/// filters, and creation time constraints. It allows the application layer
/// to construct a deterministic wallet listing query without coupling the
/// query workflow to transport specific request models.
/// </remarks>
public sealed class WalletListContext : IContext
{
    /// <summary>
    /// Unique identifier of this query context instance.
    /// </summary>
    /// <remarks>
    /// Used for correlation, diagnostics, and tracing during query execution.
    /// </remarks>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// UTC timestamp when this query context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    // Pagination

    /// <summary>
    /// Maximum number of wallet records returned in a single page.
    /// </summary>
    /// <remarks>
    /// Controls query result size and prevents unbounded wallet retrieval.
    /// </remarks>
    public int PageSize { get; init; } = 50;

    /// <summary>
    /// Opaque cursor token identifying the next page position.
    /// </summary>
    /// <remarks>
    /// Used for stable cursor-based pagination across large wallet datasets.
    /// </remarks>
    public string? PageToken { get; init; }

    // Filters

    /// <summary>
    /// Optional owner identifier used to filter wallet results.
    /// </summary>
    public string? OwnerId { get; init; }

    /// <summary>
    /// Optional wallet lifecycle status filter.
    /// </summary>
    public WalletStatus? Status { get; init; }

    /// <summary>
    /// Optional wallet classification filter.
    /// </summary>
    public WalletType? WalletType { get; init; }

    /// <summary>
    /// Optional collection of tags used for wallet classification filtering.
    /// </summary>
    /// <remarks>
    /// Wallets matching the supplied tags are included in the result set.
    /// </remarks>
    public IReadOnlyCollection<string> Tags { get; init; } = [];

    // Time range

    /// <summary>
    /// Lower the creation timestamp boundary for wallet filtering.
    /// </summary>
    /// <remarks>
    /// When specified, only wallets created at or after this timestamp
    /// are returned.
    /// </remarks>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>
    /// Upper creation timestamp boundary for wallet filtering.
    /// </summary>
    /// <remarks>
    /// When specified, only wallets created at or before this timestamp
    /// are returned.
    /// </remarks>
    public DateTimeOffset? CreatedBefore { get; init; }
}