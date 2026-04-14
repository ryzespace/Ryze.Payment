using Ryze.Domain.Exceptions;
using Ryze.Domain.Features.Shared.Enum;

namespace Ryze.Domain.Features.Wallet.ValueObject;

/// <summary>
/// Monetary value object with enforced currency and precision invariants.
/// </summary>
/// <remarks>
/// Represents an immutable amount of money bound to a specific currency
/// and decimal precision. All arithmetic operations require compatibility
/// between operands.
/// </remarks>
public readonly record struct Money(
    decimal Amount,
    Currency Currency,
    int Precision
)
{
    /// <summary>
    /// Adds another monetary value to this instance.
    /// </summary>
    /// <remarks>
    /// Both values must share the same currency and precision.
    /// </remarks>
    public Money Add(Money other)
    {
        EnsureCompatible(other);
        return this with { Amount = Amount + other.Amount };
    }

    /// <summary>
    /// Subtracts another monetary value from this instance.
    /// </summary>
    /// <remarks>
    /// Both values must share the same currency and precision.
    /// </remarks>
    public Money Subtract(Money other)
    {
        EnsureCompatible(other);
        return this with { Amount = Amount - other.Amount };
    }

    /// <summary>
    /// Ensures that the provided monetary value is compatible
    /// with this instance.
    /// </summary>
    /// <exception cref="DomainException">
    /// Thrown when currency or precision does not match.
    /// </exception>
    private void EnsureCompatible(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Currency mismatch");

        if (Precision != other.Precision)
            throw new DomainException("Precision mismatch");
    }
}