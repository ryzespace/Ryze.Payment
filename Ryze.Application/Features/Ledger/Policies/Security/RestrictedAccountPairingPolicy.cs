using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Transaction;
using ChartOfAccounts = Ryze.Domain.Features.Ledger.Entity.ChartsAccounts.ChartOfAccounts;

namespace Ryze.Application.Features.Ledger.Policies.Security;

/// <summary>
/// Policy that prevents direct postings between restricted account types
/// that could bypass required financial workflows or settlement processes.
/// </summary>
/// <remarks>
/// For example, the Platform Treasury must not be directly paired with a
/// user wallet within the same ledger transaction. Such flows must use an
/// approved intermediate account, such as a platform payable or escrow account.
/// </remarks>
public sealed class RestrictedAccountPairingPolicy
    : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.security.restricted_pairing";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 70;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Blocks restricted account combinations within a single ledger transaction.";

    /// <summary>
    /// Evaluates the policy against a mutation.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state. </param>
    /// <returns>
    /// A policy decision indicating whether the mutation is allowed or denied.
    /// </returns>
    public PolicyDecision Evaluate(
        IMutation<LedgerTransaction> mutation,
        LedgerTransaction state)
    {
        if (mutation is not RecordTransactionMutation record)
            return PolicyDecision.Allow(Name);

        var transaction = record.Transaction;

        var accountIds = transaction.Entries
            .Select(entry => entry.Account.AccountId)
            .ToList();

        // Direct postings between the Platform Treasury and user wallet
        // accounts are prohibited. Funds must flow through an approved
        // intermediate account, such as platform payable or escrow,
        // so that withdrawal and settlement workflows cannot be bypassed
        // by recording direct ledger transaction.
        var hasTreasury = accountIds.Any(
            id => id.Equals(
                ChartOfAccounts.PlatformTreasury.Id,
                StringComparison.OrdinalIgnoreCase));

        var hasUserWallet = accountIds.Any(
            id => id.StartsWith("wallet:",
                StringComparison.OrdinalIgnoreCase));

        if (hasTreasury && hasUserWallet)
        {
            return PolicyDecision.Deny(
                "SECURITY VIOLATION: Direct transfer between Platform Treasury " +
                "and user wallet is forbidden. Use an intermediate account " +
                "(Escrow/Payable).",
                Name);
        }

        return PolicyDecision.Allow(Name);
    }
}