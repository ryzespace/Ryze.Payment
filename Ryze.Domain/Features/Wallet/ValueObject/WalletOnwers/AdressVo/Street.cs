namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

/// <summary>
/// Street value within a wallet owner address.
/// </summary>
/// <remarks>
/// Domain value object representing a street name or identifier.
/// </remarks>
public readonly record struct Street(string Value)
{
    /// <summary>
    /// Returns the street value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the street value to its string representation.
    /// </summary>
    public static implicit operator string(Street street) => street.Value;

    /// <summary>
    /// Creates a <see cref="Street"/> from a string value.
    /// </summary>
    public static implicit operator Street(string value) => new(value);
}
