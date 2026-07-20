using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Contexts;

/// <summary>
/// Context describing a wallet owner definition during wallet operations.
/// </summary>
/// <remarks>
/// Represents ownership data required when creating or modifying wallet
/// ownership information.
///
/// The context captures identity attributes, ownership role, address data,
/// compliance-related information, and additional metadata. It is consumed
/// by wallet creation and mutation workflows to construct or update the
/// corresponding domain owner entity.
/// </remarks>
public sealed class WalletOwnerContext : IContext
{
    /// <summary>
    /// Unique identifier of this owner context instance.
    /// </summary>
    /// <remarks>
    /// Used for correlation and diagnostics within the wallet operation pipeline.
    /// </remarks>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// UTC timestamp when this owner context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the owner entity.
    /// </summary>
    /// <remarks>
    /// Can represent a user, organization, or external identity provider subject
    /// depending on the wallet ownership model.
    /// </remarks>
    public required string OwnerId { get; init; }

    /// <summary>
    /// Human-readable name used for display and identification purposes.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Defines the owner's permissions and responsibility within the wallet.
    /// </summary>
    public required OwnerRole Role { get; init; }

    /// <summary>
    /// Postal address associated with the wallet owner.
    /// </summary>
    /// <remarks>
    /// The address is provided as an application DTO and is transformed into
    /// the corresponding domain representation during the wallet workflow.
    /// </remarks>
    public required AddressDto Address { get; init; }

    /// <summary>
    /// Optional date of birth used for identity verification workflows.
    /// </summary>
    public DateTime? DateOfBirth { get; init; }

    /// <summary>
    /// Optional government-issued identifier used for compliance checks.
    /// </summary>
    /// <remarks>
    /// Handling and storage requirements depend on the active compliance policy.
    /// </remarks>
    public string? NationalId { get; init; }

    /// <summary>
    /// Additional owner-specific metadata.
    /// </summary>
    /// <remarks>
    /// Contains extensible attributes that do not belong to the core owner
    /// identity model.
    /// </remarks>
    public Dictionary<string, string> Metadata { get; init; } = new();
}