using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.ValueObject;

namespace Ryze.Domain.Features.Ledger.Interfaces;

/// <summary>
/// Defines validation rules for foreign exchange operations inside the ledger.
/// </summary>
/// <remarks>
/// This validator ensures that multi currency ledger transactions are
/// economically consistent with the exchange rates used during conversion.
///
/// It validates the relationship between amounts posted in different currencies
/// and detects inconsistencies caused by invalid rates, incorrect calculations,
/// or unexpected currency exposure.
///
/// This validation is separate from basic double entry validation because
/// debit/credit balancing alone does not guarantee economic correctness
/// for cross currency transactions.
/// </remarks>
public interface IForeignExchangeValidator
{
    /// <summary>
    /// Validates economic consistency of multi currency ledger transaction.
    /// </summary>
    /// <remarks>
    /// The validator compares transaction postings across currencies against
    /// provided exchange rates and returns any detected FX inconsistencies.
    /// </remarks>
    /// <param name="transaction">Ledger transaction containing currency postings to validate. </param>
    /// <param name="exchangeRates">Exchange rates applicable to currencies involved in the transaction.</param>
    /// <returns>Result containing validation status and any FX related errors. </returns>
    ForeignExchangeValidationResult Validate(
        LedgerTransaction transaction,
        IReadOnlyList<ExchangeRate> exchangeRates);
}

/// <summary>
/// Represents the outcome of foreign exchange validation.
/// </summary>
/// <remarks>
/// Contains the overall validation status and detailed information
/// about detected FX inconsistencies.
///
/// A valid result contains no errors.
/// </remarks>
public sealed record ForeignExchangeValidationResult
{
    /// <summary>
    /// Indicates whether the transaction passed FX validation.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Collection of FX validation errors detected during processing.
    /// </summary>
    /// <remarks>
    /// Empty collection means that no FX inconsistencies were detected.
    /// </remarks>
    public IReadOnlyList<ForeignExchangeError> Errors { get; init; } = [];
}

/// <summary>
/// Represents single foreign exchange validation issue.
/// </summary>
/// <remarks>
/// Provides diagnostic information about a specific currency-related
/// inconsistency detected during transaction validation.
/// </remarks>
public sealed record ForeignExchangeError
{
    /// <summary>
    /// Currency code related to the validation error.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Description of the validation failure.
    /// </summary>
    public required string Message { get; init; }
}
