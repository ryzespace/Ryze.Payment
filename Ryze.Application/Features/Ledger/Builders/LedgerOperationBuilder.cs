using Ryze.Domain.Features.Ledger.Entity;
using Ryze.Domain.Features.Ledger.Enum;

namespace Ryze.Application.Features.Ledger.Builders;

/// <summary>
/// Provides fluent builder for creating ledger business operations.
/// </summary>
/// <remarks>
/// Simplifies the creation of common financial operations such as transfers,
/// deposits, withdrawals, fees, refunds, and balance adjustments.
///
/// The builder provides a business oriented API over low level ledger entry
/// creation, allowing application services to describe financial intent
/// without managing individual debit and credit entries directly.
///
/// All operations are converted into balanced ledger transactions that preserve
/// double-entry accounting principles.
/// </remarks>
public sealed partial class LedgerOperationBuilder
{
    private readonly LedgerTransactionBuilder _builder;

    private LedgerOperationBuilder(TransactionType type, string initiatedBy)
    {
        _builder = LedgerTransactionBuilder.Create(type, initiatedBy);
    }

    /// <summary>
    /// Starts building a new ledger business operation.
    /// </summary>
    /// <param name="type">Type of ledger transaction being created.</param>
    /// <param name="initiatedBy">Identifier of the actor initiating the operation.</param>
    /// <returns>A new operation builder instance.</returns>
    public static LedgerOperationBuilder Start(
        TransactionType type,
        string initiatedBy)
        => new(type, initiatedBy);

    /// <summary>
    /// Defines an idempotency key for the operation.
    /// </summary>
    /// <remarks>
    /// The key is used to safely identify repeated requests and prevent
    /// duplicate financial operations.
    /// </remarks>
    /// <param name="key">Unique idempotency identifier.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder WithIdempotencyKey(string key)
    {
        _builder.WithIdempotencyKey(key);
        return this;
    }

    /// <summary>
    /// Defines the business reason associated with the operation.
    /// </summary>
    /// <param name="reason">Human-readable operation reason.</param>
    /// <returns>The current operation builder instance.</returns>
    public LedgerOperationBuilder WithReason(string reason)
    {
        _builder.WithReason(reason);
        return this;
    }

    /// <summary>
    /// Creates the final ledger transaction.
    /// </summary>
    /// <remarks>
    /// The resulting transaction is validated before being returned and must
    /// satisfy ledger consistency rules including double-entry balancing.
    /// </remarks>
    /// <returns>A completed ledger transaction.</returns>
    public LedgerTransaction Build()
        => _builder.Build();

    /// <summary>
    /// Determines whether an adjustment operation increases an account balance.
    /// </summary>
    /// <param name="adjustmentType">Adjustment classification.</param>
    /// <returns>
    /// <c>true</c> when the adjustment represents an increase;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool ShouldIncrease(string adjustmentType)
    {
        return adjustmentType.Contains("increase", StringComparison.OrdinalIgnoreCase)
            || adjustmentType.Contains("credit", StringComparison.OrdinalIgnoreCase)
            || adjustmentType.Contains("add", StringComparison.OrdinalIgnoreCase)
            || adjustmentType.Contains("refund", StringComparison.OrdinalIgnoreCase)
            || adjustmentType.Contains("topup", StringComparison.OrdinalIgnoreCase);
    }
}