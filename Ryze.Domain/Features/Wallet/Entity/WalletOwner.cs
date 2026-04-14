using Ryze.Domain.Features.Shared.Enum;
using Ryze.Domain.Features.Wallet.ValueObject;
using Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.WalletVo;

namespace Ryze.Domain.Features.Wallet.Entity;

/// <summary>
/// Domain entity representing a wallet owner identity and profile.
/// </summary>
/// <remarks>
/// Wallet owner stores role assignment, address and optional compliance metadata
/// used by onboarding and lifecycle operations.
/// </remarks>
public sealed class WalletOwner
{
    /// <summary>
    /// External owner identifier.
    /// </summary>
    public string OwnerId { get; set; } = null!;

    /// <summary>
    /// Human-readable owner display name.
    /// </summary>
    public DisplayName DisplayName { get; set; } = null!;

    /// <summary>
    /// Owner role within wallet permissions model.
    /// </summary>
    public OwnerRole Role { get; set; }

    /// <summary>
    /// Owner address information.
    /// </summary>
    public Address Address { get; set; } = null!;

    /// <summary>
    /// Optional owner date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Optional national identifier.
    /// </summary>
    public NationalId? NationalId { get; set; }

    /// <summary>
    /// Timestamp when owner was attached to wallet.
    /// </summary>
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Additional owner metadata.
    /// </summary>
    public Metadata Metadata { get; set; } = Metadata.Empty;
    
    /// <summary>
    /// Changes owner role.
    /// </summary>
    /// <param name="role">Target role value.</param>
    public void ChangeRole(OwnerRole role)
        => Role = role;
    
    /// <summary>
    /// Replaces owner address with a new instance.
    /// </summary>
    /// <param name="address">New address value.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="address"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateAddress(Address address) => Address = address ?? throw new ArgumentNullException(nameof(address));

    /// <summary>
    /// Updates metadata with provided key-value pair.
    /// </summary>
    /// <param name="key">Metadata key.</param>
    /// <param name="value">Metadata value.</param>
    public void UpdateMetadata(string key, string value) =>
        Metadata = Metadata.With(key, value);
}
