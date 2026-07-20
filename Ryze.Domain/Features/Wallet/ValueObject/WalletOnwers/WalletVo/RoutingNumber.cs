namespace Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.WalletVo;

/// <summary>
/// Routing number value for a wallet.
/// </summary>
/// <remarks>
/// Domain value object representing a bank routing identifier.
/// </remarks>
public readonly record struct RoutingNumber(string Value)
{
    /// <summary>
    /// Returns the routing number value.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Converts the routing number value to its string representation.
    /// </summary>
    public static implicit operator string(RoutingNumber routingNumber) => routingNumber.Value;

    /// <summary>
    /// Creates a <see><cref>RoutingNumber</cref>
    /// </see>
    /// from a string value.
    /// </summary>
    public static implicit operator RoutingNumber(string value) => new(value);
}
