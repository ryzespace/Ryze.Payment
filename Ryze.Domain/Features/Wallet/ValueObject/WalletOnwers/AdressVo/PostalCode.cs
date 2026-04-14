namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

/// <summary>
/// Postal code value within a wallet owner address.
/// </summary>
/// <remarks>
/// Domain value object representing a postal code identifier.
/// </remarks>
public readonly record struct PostalCode(string Value)
{
    /// <summary>
    /// Returns the postal code value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the postal code value to its string representation.
    /// </summary>
    public static implicit operator string(PostalCode postalCode) => postalCode.Value;

    /// <summary>
    /// Creates a <see cref="PostalCode"/> from a string value.
    /// </summary>
    public static implicit operator PostalCode(string value) => new(value);
}
