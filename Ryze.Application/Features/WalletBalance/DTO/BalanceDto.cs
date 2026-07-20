namespace Ryze.Application.Features.WalletBalance.DTO;

/// <summary>
/// Represents a snapshot of wallet balances and the time it was last updated.
/// </summary>
/// <remarks>
/// - <see cref="AvailableBalance"/>: funds immediately available for use
/// - <see cref="CurrentBalance"/>: total ledger balance
/// - <see cref="FrozenBalance"/>: funds on hold and not currently spendable
/// - <see cref="PendingBalance"/>: funds pending settlement/confirmation
/// </remarks>
public sealed class BalanceDto
{
    /// <summary>
    /// Gets the amount that is immediately available for spending or withdrawal.
    /// </summary>
    public MoneyDto AvailableBalance { get; init; } = null!;

    /// <summary>
    /// Gets the total ledger balance, including available, pending, and frozen amounts.
    /// </summary>
    public MoneyDto CurrentBalance { get; init; } = null!;

    /// <summary>
    /// Gets the amount that is currently on hold (frozen) and not available for use.
    /// </summary>
    public MoneyDto FrozenBalance { get; init; } = null!;

    /// <summary>
    /// Gets the amount that is pending settlement or confirmation and not yet available.
    /// </summary>
    public MoneyDto PendingBalance { get; init; } = null!;

    /// <summary>
    /// Gets the timestamp indicating when the balance values were last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}
