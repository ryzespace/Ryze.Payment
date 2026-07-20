namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

/// <summary>
/// Secondary street value within a wallet owner address.
/// </summary>
/// <remarks>
/// Domain value object representing an additional street line.
/// </remarks>
public readonly record struct StreetTwo(string Value)
{
    /// <summary>
    /// Returns the secondary street value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the secondary street value to its string representation.
    /// </summary>
    public static implicit operator string(StreetTwo streetTwo) => streetTwo.Value;

    /// <summary>
    /// Creates a <see cref="StreetTwo"/> from a string value.
    /// </summary>
    public static implicit operator StreetTwo(string value) => new(value);
}