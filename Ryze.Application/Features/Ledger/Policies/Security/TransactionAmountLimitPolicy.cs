using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;

namespace Ryze.Application.Features.Ledger.Policies.Security;

/// <summary>
/// Prevents ledger transactions from containing excessively large posting amounts.
/// </summary>
/// <remarks>
/// Protects the ledger against unusually large values caused by application errors,
/// malformed requests, or potentially malicious input by enforcing a maximum amount
/// per journal entry.
/// </remarks>
public sealed class TransactionAmountLimitPolicy : IMutationPolicy<LedgerTransaction>
{
    private const decimal MaxTransactionAmount = 1_000_000_000_000m; // 1 Trillion - "common sense" safety limit

    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.security.amount_limit";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 40;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Verifies that transaction amounts do not exceed safe system thresholds.";

    /// <summary>
    /// Evaluates the specified mutation against the maximum transaction amount limit.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A policy decision that denies the mutation when any journal entry exceeds
    /// the configured maximum amount; otherwise, an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<LedgerTransaction> mutation, LedgerTransaction state)
    {
        if (mutation is not RecordTransactionMutation record) return PolicyDecision.Allow(Name);
        
        var tx = record.Transaction;
        
        foreach (var entry in tx.Entries)
        {
            if (entry.Posting.Amount > MaxTransactionAmount)
            {
                return PolicyDecision.Deny($"Transaction amount ({entry.Posting.Amount}) exceeds the allowed safety limit ({MaxTransactionAmount}).", Name);
            }
        }

        return PolicyDecision.Allow(Name);
    }
}
