using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using ModularityKit.Mutator.Abstractions.Context;
using Ryze.Application.Features.Ledger.Mutations.Transaction;

namespace Ryze.Application.Features.Ledger.Policies.Security;

/// <summary>
/// Enforces the presence of an idempotency key when recording ledger transactions in commit mode.
/// </summary>
/// <remarks>
/// Prevents duplicate transaction recording when committed operation is retried by requiring
/// every <see cref="RecordTransactionMutation"/> executed in <see cref="MutationMode.Commit"/>
/// mode to provide non empty idempotency key.
/// </remarks>
public sealed class MandatoryIdempotencyKeyPolicy : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.security.mandatory_idempotency";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 30;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Enforces idempotency key for transactions in Commit mode.";

    /// <summary>
    /// Evaluates the specified mutation against the idempotency key requirement.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A policy decision that denies the mutation when a transaction is being recorded
    /// in commit mode without a valid idempotency key; otherwise, an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<LedgerTransaction> mutation, LedgerTransaction state)
    {
        if (mutation.Context.Mode != MutationMode.Commit || mutation is not RecordTransactionMutation record)
            return PolicyDecision.Allow(Name);

        return string.IsNullOrWhiteSpace(record.Transaction.IdempotencyKey)
            ? PolicyDecision.Deny("SECURITY RISK: Transactions in Commit mode must have an IdempotencyKey.", Name)
            : PolicyDecision.Allow(Name);
    }
}
