using Ryze.Domain.Shared.Enum;

namespace Ryze.Domain.Features.Ledger.ValueObject;

/// <summary>
/// Represents foreign exchange conversion rate between two currencies.
/// </summary>
/// <remarks>
/// Exchange rates are immutable and validated before being used by ledger operations.
/// They are primarily used for multi currency transactions, FX conversion,
/// and reconciliation processes.
/// </remarks>
public sealed record ExchangeRate
{
    /// <summary>
    /// Currency being converted from.
    /// </summary>
    /// <remarks>
    /// Represents the source currency in the exchange operation.
    /// </remarks>
    public required Currency BaseCurrency { get; init; }

    /// <summary>
    /// Currency being converted to.
    /// </summary>
    /// <remarks>
    /// Represents the target currency used to express the conversion value.
    /// </remarks>
    public required Currency QuoteCurrency { get; init; }

    /// <summary>
    /// Exchange conversion multiplier.
    /// </summary>
    /// <remarks>
    /// Defines how many units of <see cref="QuoteCurrency"/> equal one unit
    /// of <see cref="BaseCurrency"/>.
    ///
    /// Example:
    /// <code>
    /// BaseCurrency = EUR
    /// QuoteCurrency = USD
    /// Rate = 1.10
    ///
    /// 100 EUR = 110 USD
    /// </code>
    /// </remarks>
    public required decimal Rate { get; init; }

    /// <summary>
    /// Maximum allowed percentage deviation during exchange validation.
    /// </summary>
    /// <remarks>
    /// Used to tolerate small differences caused by rounding,
    /// provider precision, or settlement timing differences.
    /// </remarks>
    public decimal TolerancePercent { get; init; } = 1m;

    /// <summary>
    /// Timestamp when the exchange rate was captured.
    /// </summary>
    /// <remarks>
    /// Used for auditability and historical reconstruction of
    /// multi currency ledger transactions.
    /// </remarks>
    public DateTimeOffset ObservedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Validates the exchange rate consistency.
    /// </summary>
    /// <remarks>
    /// Invalid values indicate a domain inconsistency and should prevent
    /// further processing of the exchange operation.
    /// </remarks>
    public void Validate()
    {
        if (Rate <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(Rate),
                "Exchange rate must be positive.");

        if (TolerancePercent < 0)
            throw new ArgumentOutOfRangeException(
                nameof(TolerancePercent),
                "Tolerance cannot be negative.");

        if (BaseCurrency == Currency.Unspecified)
            throw new ArgumentException(
                "Base currency must be specified.",
                nameof(BaseCurrency));

        if (QuoteCurrency == Currency.Unspecified)
            throw new ArgumentException(
                "Quote currency must be specified.",
                nameof(QuoteCurrency));

        if (BaseCurrency == QuoteCurrency)
            throw new ArgumentException(
                "Base and quote currency must differ.");
    }
}