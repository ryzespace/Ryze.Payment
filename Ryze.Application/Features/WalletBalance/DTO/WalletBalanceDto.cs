namespace Ryze.Application.Features.WalletBalance.DTO;

/// <summary>
/// Data transfer object representing wallet balance snapshot.
/// </summary>
/// <remarks>
/// Contains flattened balance values used by application and transport layers.
/// </remarks>
public sealed class WalletBalanceDto
{
    /// <summary>
    /// Amount currently available for spending or withdrawal.
    /// </summary>
    public decimal Available { get; init; }

    /// <summary>
    /// Amount currently reserved/held and not available for spending.
    /// </summary>
    public decimal Reserved { get; init; }

    /// <summary>
    /// ISO-like currency code associated with the balance values.
    /// </summary>
    public string Currency { get; init; } = null!;
}
