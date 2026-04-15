using Ryze.Domain.Features.Shared.Enum;

namespace Ryze.Application.Features.Walllet.DTO;

/// <summary>
/// Data Transfer Object representing a wallet owner.
/// </summary>
/// <remarks>
/// <para>
/// Properties include identity, role, address, optional personal information,
/// creation timestamp, and extensible metadata.
/// </para>
/// </remarks>
public sealed class WalletOwnerDto
{
    /// <summary>
    /// Unique identifier of the wallet owner.
    /// </summary>
    public string OwnerId { get; init; } = null!;

    /// <summary>
    /// The role of the wallet owner in the system.
    /// </summary>
    public OwnerRole Role { get; init; }

    /// <summary>
    /// Optional address information of the wallet owner.
    /// </summary>
    public AddressDto? Address { get; init; }

    /// <summary>
    /// Optional date of birth of the wallet owner.
    /// </summary>
    public DateTimeOffset? DateOfBirth { get; init; }

    /// <summary>
    /// Optional national ID of the wallet owner.
    /// </summary>
    public string? NationalId { get; init; }

    /// <summary>
    /// Timestamp when the owner was added to the wallet.
    /// </summary>
    public DateTimeOffset AddedAt { get; init; }

    /// <summary>
    /// Optional metadata associated with the wallet owner.
    /// Keys are expected to be normalized (lowercase, trimmed).
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
