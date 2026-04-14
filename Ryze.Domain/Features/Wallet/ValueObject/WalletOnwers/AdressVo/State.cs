namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.AdressVo;

/// <summary>
/// State value within a wallet owner address.
/// </summary>
/// <remarks>
/// Domain value object representing a state or region.
/// </remarks>
public readonly record struct State(string Value)
{
    /// <summary>
    /// Returns the state value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the state value to its string representation.
    /// </summary>
    public static implicit operator string(State state) => state.Value;

    /// <summary>
    /// Creates a <see cref="State"/> from a string value.
    /// </summary>
    public static implicit operator State(string value) => new(value);
}
