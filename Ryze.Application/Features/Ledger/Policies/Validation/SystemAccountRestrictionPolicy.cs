using Ryze.Domain.Features.Ledger.Entity;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Ledger.Mutations.Operations;
using ChartOfAccounts = Ryze.Domain.Features.Ledger.Entity.ChartsAccounts.ChartOfAccounts;

namespace Ryze.Application.Features.Ledger.Policies.Validation;

/// <summary>
/// Restricts direct ledger operations involving sensitive system control accounts.
/// </summary>
/// <remarks>
/// Prevents operations from directly modifying or transferring
/// funds through protected system accounts such as suspense and currency exchange accounts.
/// These accounts are intended to be managed by dedicated system workflows.
/// </remarks>
public sealed class SystemAccountRestrictionPolicy : IMutationPolicy<LedgerTransaction>
{
    /// <summary>
    /// Gets the unique name of the policy.
    /// </summary>
    public string Name =>
        "ledger.validation.system_account";

    /// <summary>
    /// Gets the priority of the policy.
    /// </summary>
    public int Priority => 140;

    /// <summary>
    /// Gets the description of the policy.
    /// </summary>
    public string Description =>
        "Restricts direct operations on sensitive system control accounts";

    private static readonly HashSet<string> RestrictedAccounts =
    [
        ChartOfAccounts.SuspenseAccount.Id,
        ChartOfAccounts.CurrencyExchangeGainLoss.Id
    ];

    /// <summary>
    /// Evaluates the specified mutation against system account restrictions.
    /// </summary>
    /// <param name="mutation">The mutation being evaluated.</param>
    /// <param name="state">The current ledger transaction state.</param>
    /// <returns>
    /// A policy decision that denies direct operations or transfers involving restricted
    /// system accounts; otherwise, an allow decision.
    /// </returns>
    public PolicyDecision Evaluate(
        IMutation<LedgerTransaction> mutation,
        LedgerTransaction state)
    {
        var targetAccount = mutation switch
        {
            DepositMutation deposit => deposit.Input.AccountId,
            FeeMutation fee => fee.Input.FromAccountId,
            AdjustmentMutation adjustment => adjustment.Input.AccountId,
            _ => null
        };

        if (targetAccount is not null &&
            RestrictedAccounts.Contains(targetAccount))
        {
            return PolicyDecision.Deny(
                $"Direct operation on system control account '{targetAccount}' is restricted.", Name);
        }

        if (mutation is not TransferMutation transfer)
        {
            return PolicyDecision.Allow(Name);
        }

        if (RestrictedAccounts.Contains(transfer.Input.FromAccountId) ||
            RestrictedAccounts.Contains(transfer.Input.ToAccountId))
        {
            return PolicyDecision.Deny(
                "Direct transfers involving system control accounts are restricted.", Name);
        }

        return PolicyDecision.Allow(Name,
            "Operation does not directly target restricted system control account.");
    }
}
