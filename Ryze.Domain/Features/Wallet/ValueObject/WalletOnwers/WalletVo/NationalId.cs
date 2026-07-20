namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.WalletVo;

/// <summary>
/// National ID value for a wallet owner or related entity.
/// </summary>
/// <remarks>
/// Domain value object representing unique identifier (e.g., SSN, NIP).
/// </remarks>
public readonly record struct NationalId(string Value)
{
    /// <summary>
    /// Returns the national ID value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the national ID value to its string representation.
    /// </summary>
    public static implicit operator string(NationalId nationalId) => nationalId.Value;

    /// <summary>
    /// Creates a <see cref="NationalId"/> from a string value.
    /// </summary>
    public static implicit operator NationalId(string value) => new(value);
}
