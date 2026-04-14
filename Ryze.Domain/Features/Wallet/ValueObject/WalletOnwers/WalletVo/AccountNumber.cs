namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.WalletVo;

/// <summary>
/// Account number value for a wallet.
/// </summary>
/// <remarks>
/// Domain value object representing a wallet's account identifier.
/// </remarks>
public readonly record struct AccountNumber(string Value)
{
    /// <summary>
    /// Returns the account number value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the account number value to its string representation.
    /// </summary>
    public static implicit operator string(AccountNumber accountNumber) => accountNumber.Value;

    /// <summary>
    /// Creates an <see> <cref>AccountNumber</cref>
    /// </see>
    /// from a string value.
    /// </summary>
    public static implicit operator AccountNumber(string value) => new(value);
}
