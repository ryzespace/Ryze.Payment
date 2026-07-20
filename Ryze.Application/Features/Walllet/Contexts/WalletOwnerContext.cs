using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Domain.Features.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Write context representing a wallet owner definition.
/// </summary>
/// <remarks>
/// Describes ownership, role, and identity attributes assigned
/// to a wallet owner at creation or update time.
/// </remarks>
public sealed class WalletOwnerContext : IContext
{
    /// <summary>
    /// Unique identifier of the owner context.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Timestamp of context creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the owner (user, organization, or external entity).
    /// </summary>
    public required string OwnerId { get; init; }

    /// <summary>
    /// Human-readable display name of the owner.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Role of the owner within the wallet.
    /// </summary>
    public required OwnerRole Role { get; init; }

    /// <summary>
    /// Address information associated with the owner.
    /// </summary>
    public required AddressDto Address { get; init; }

    /// <summary>
    /// Optional date of birth for identity verification.
    /// </summary>
    public DateTime? DateOfBirth { get; init; }

    /// <summary>
    /// Optional national identifier for compliance purposes.
    /// </summary>
    public string? NationalId { get; init; }

    /// <summary>
    /// Arbitrary metadata associated with the wallet owner.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}
