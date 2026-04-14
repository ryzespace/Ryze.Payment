namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.WalletVo;

/// <summary>
/// Display name value for related wallet.
/// </summary>
/// <remarks>
/// Domain value object representing a human-readable name.
/// </remarks>
public readonly record struct DisplayName(string Value)
{
    /// <summary>
    /// Returns the display name value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the display name value to its string representation.
    /// </summary>
    public static implicit operator string(DisplayName displayName) => displayName.Value;

    /// <summary>
    /// Creates a <see cref="DisplayName"/> from a string value.
    /// </summary>
    public static implicit operator DisplayName(string value) => new(value);
}
