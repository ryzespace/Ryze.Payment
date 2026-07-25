using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;

namespace Ryze.Application.Features.Ledger.Policies.Validation;

/// <summary>
/// Prevents ledger transactions from being recorded with timestamps too far in the future.
/// </summary>
/// <remarks>
/// Allows small clock drift tolerance to account for minor differences between system clocks
/// while preventing transactions from being recorded with invalid future timestamps.
/// </remarks>
public sealed class TransactionTimestampPolicy : IMutationPolicy<LedgerTransaction>
{
    private static readonly TimeSpan ClockDriftTolerance =
        TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.validation.timestamp";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 120;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Ensures ledger transactions do not have future timestamps";

    /// <summary>
    /// Evaluates the specified mutation against the transaction timestamp rules.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// Policy decision that denies the mutation when the transaction timestamp exceeds
    /// the current UTC time by more than the configured clock drift tolerance; otherwise,
    /// an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<LedgerTransaction> mutation, LedgerTransaction state)
    {
        if (mutation is not RecordTransactionMutation record) return PolicyDecision.Allow(Name);
        
        var transaction = record.Transaction;
        var maximumAllowedTimestamp =
            DateTimeOffset.UtcNow + ClockDriftTolerance;

        return transaction.Timestamp > maximumAllowedTimestamp
            ? PolicyDecision.Deny(
                "Ledger transaction cannot be recorded with a future timestamp.", Name)
            : PolicyDecision.Allow(Name);
    }
}
