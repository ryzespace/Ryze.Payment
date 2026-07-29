using ModularityKit.Mutator.Abstractions.Context;
using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;
using Ryze.Application.Shared.Helpers;

namespace Ryze.Application.Features.Ledger.Policies.Security;

/// <summary>
/// Protects system owned ledger accounts from unauthorized transaction postings.
/// </summary>
/// <remarks>
/// Prevents transactions initiated by non system actors from posting entries to
/// protected platform accounting segments. System actors identified by the
/// <c>system:</c> prefix are permitted to access these accounts.
/// </remarks>
public sealed class ProtectedAccountIntegrityPolicy : IMutationPolicy<LedgerTransaction>
{
    private static readonly string[] ProtectedSegments =
    [
        "asset:platform",
        "liability:platform",
        "equity:platform",
        "revenue:platform",
        "expense:platform"
    ];

    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.integrity.protected_accounts";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 60;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "A ledger protection ensuring user contexts cannot manipulate system reserve accounts inappropriately.";

    /// <summary>
    /// Evaluates the specified mutation against protected account access rules.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A policy decision that denies the mutation when non system actor attempts
    /// to post to protected account; otherwise, an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<LedgerTransaction> mutation, LedgerTransaction state)
    {
        var entries = mutation switch
        {
            RecordTransactionMutation r => r.Transaction.Entries,
            ReverseTransactionMutation r => r.Ctx.OriginalTransaction.Entries,
            _ when state is not null => state.Entries,
            _ => null
        };

        if (entries is null)
            return PolicyDecision.Allow(Name);

        if (mutation.Context.ActorType is ActorType.System
            or ActorType.Service
            or ActorType.Administrator)
            return PolicyDecision.Allow(Name, "Transaction passes protected account integrity policy.");

        var touchesProtected = entries.Any(e =>
            ProtectedSegments.Any(s => e.Account.AccountId.StartsWith(s, StringComparison.OrdinalIgnoreCase)));

        if (touchesProtected)
        {
            return PolicyDecision.Deny(
                "SECURITY VIOLATION: Non-system actors cannot book entries against core platform accounting segments.",
                Name);
        }

        return PolicyDecision.Allow(Name, "Transaction passes protected account integrity policy.");
    }
}
