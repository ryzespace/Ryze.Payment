using Ryze.Domain.Exceptions;
using Ryze.Domain.Features.Wallet.ValueObject;

namespace Ryze.Domain.Features.WalletBalance.Entity;

/// <summary>
/// Domain entity representing wallet monetary buckets.
/// </summary>
/// <remarks>
/// Maintains balance split across available, current, frozen, and pending amounts
/// while enforcing currency/precision compatibility across operations.
/// </remarks>
public sealed class WalletBalance
{
    /// <summary>
    /// Funds that can be immediately spent or withdrawn.
    /// </summary>
    public Money Available { get; private set; }

    /// <summary>
    /// Total funds currently recognized by the wallet.
    /// </summary>
    public Money Current { get; private set; }

    /// <summary>
    /// Funds temporarily blocked from spending.
    /// </summary>
    public Money Frozen { get; private set; }

    /// <summary>
    /// Funds awaiting settlement.
    /// </summary>
    public Money Pending { get; private set; }

    /// <summary>
    /// Last balance update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes wallet balance with explicit bucket values.
    /// </summary>
    /// <param name="available">Available amount.</param>
    /// <param name="current">Current total amount.</param>
    /// <param name="frozen">Frozen amount.</param>
    /// <param name="pending">Pending amount.</param>
    /// <param name="updatedAt">Initial update timestamp.</param>
    /// <exception cref="DomainException">
    /// Thrown when any money bucket has different currency or precision.
    /// </exception>
    public WalletBalance(
        Money available,
        Money current,
        Money frozen,
        Money pending,
        DateTime updatedAt
    )
    {
        EnsureSameCurrency(available, current, frozen, pending);

        Available = available;
        Current   = current;
        Frozen    = frozen;
        Pending   = pending;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Applies top-up operation to wallet balance.
    /// </summary>
    /// <param name="amount">Amount to add.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="amount"/> currency/precision is incompatible.
    /// </exception>
    public void ApplyTopUp(Money amount)
    {
        EnsureCompatible(amount, Available);

        Current   = Current.Add(amount);
        Available = Available.Add(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Moves funds from available bucket to frozen bucket.
    /// </summary>
    /// <param name="amount">Amount to freeze.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="amount"/> currency/precision is incompatible.
    /// </exception>
    public void Freeze(Money amount)
    {
        EnsureCompatible(amount, Available);

        Available = Available.Subtract(amount);
        Frozen    = Frozen.Add(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ensures two money values share the same currency and precision.
    /// </summary>
    /// <param name="a">First money operand.</param>
    /// <param name="b">Second money operand.</param>
    /// <exception cref="DomainException">Thrown on incompatible money operands.</exception>
    private static void EnsureCompatible(Money a, Money b)
    {
        if (a.Currency != b.Currency || a.Precision != b.Precision)
            throw new DomainException("Incompatible money operation");
    }

    /// <summary>
    /// Ensures all provided money buckets share the same currency and precision.
    /// </summary>
    /// <param name="monies">Money buckets to validate.</param>
    /// <exception cref="DomainException">Thrown on currency/precision mismatch.</exception>
    private static void EnsureSameCurrency(params Money[] monies)
    {
        var first = monies[0];

        if (monies.Any(m => m.Currency != first.Currency || m.Precision != first.Precision))
        {
            throw new DomainException("Wallet balance currency mismatch");
        }
    }
}
