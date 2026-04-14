namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

/// <summary>
/// Country value within a wallet owner address.
/// </summary>
/// <remarks>
/// Domain value object representing a country identifier.
/// </remarks>
public readonly record struct Country(string Value)
{
    /// <summary>
    /// Returns the country value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the country value to its string representation.
    /// </summary>
    public static implicit operator string(Country country) => country.Value;

    /// <summary>
    /// Creates a <see cref="Country"/> from a string value.
    /// </summary>
    public static implicit operator Country(string value) => new(value);
}
