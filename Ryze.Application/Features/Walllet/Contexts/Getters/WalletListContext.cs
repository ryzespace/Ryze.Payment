using ModularityKit.Context.Abstractions;
using Ryze.Domain.Features.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Contexts.Getters;

/// <summary>
/// Read context for wallet listing operations.
/// </summary>
/// <remarks>
/// Represents an immutable query intent defining pagination,
/// filtering, and time-based constraints for wallet listings.
/// </remarks>
public sealed class WalletListContext : IContext
{
    /// <summary>
    /// Unique identifier of the request context.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Timestamp of context creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    // Pagination

    /// <summary>
    /// Maximum number of wallets returned in a single page.
    /// </summary>
    public int PageSize { get; init; } = 50;

    /// <summary>
    /// Opaque token used to retrieve the next page.
    /// </summary>
    public string? PageToken { get; init; }

    // Filters

    /// <summary>
    /// Optional owner identifier filter.
    /// </summary>
    public string? OwnerId { get; init; }

    /// <summary>
    /// Optional wallet status filter.
    /// </summary>
    public WalletStatus? Status { get; init; }

    /// <summary>
    /// Optional wallet type filter.
    /// </summary>
    public WalletType? WalletType { get; init; }

    /// <summary>
    /// Optional tag-based filtering.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = [];

    // Time range

    /// <summary>
    /// Filters wallets created after the specified timestamp.
    /// </summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>
    /// Filters wallets created before the specified timestamp.
    /// </summary>
    public DateTimeOffset? CreatedBefore { get; init; }
}
