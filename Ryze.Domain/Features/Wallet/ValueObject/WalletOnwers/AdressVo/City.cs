namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

/// <summary>
/// City value within a wallet owner address.
/// </summary>
/// <remarks>
/// Domain value object representing a city name.
/// </remarks>
public readonly record struct City(string Value)
{
    /// <summary>
    /// Returns the city value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the city value to its string representation.
    /// </summary>
    public static implicit operator string(City city) => city.Value;

    /// <summary>
    /// Creates a <see cref="City"/> from a string value.
    /// </summary>
    public static implicit operator City(string value) => new(value);
}
