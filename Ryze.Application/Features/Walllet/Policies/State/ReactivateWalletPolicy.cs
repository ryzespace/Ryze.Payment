using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Walllet.Mutations.State;
using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Policies.State;

/// <summary>
/// Mutation policy validating whether a wallet can transition back into an active state.
/// </summary>
/// <remarks>
/// Protects wallet lifecycle integrity by enforcing rules before executing
/// <see cref="ReactivateWalletMutation"/>.
///
/// The policy ensures that only suspended wallets can be restored and prevents
/// invalid lifecycle transitions such as reactivating already active or
/// permanently closed wallets.
///
/// A reactivation reason is required to maintain an explicit operational and
/// audit context for restoring wallet availability.
///
/// This policy only evaluates wallet reactivation mutations and does not modify
/// the aggregate state.
/// </remarks>
public sealed class ReactivateWalletPolicy : IMutationPolicy<Wallet>
{
    public string Name => "wallet.reactivate";
    public int Priority => 100;
    public string Description => "Validates whether a wallet can be reactivated";

    /// <summary>
    /// Evaluates whether wallet reactivation is allowed.
    /// </summary>
    /// <remarks>
    /// Checks the current wallet lifecycle state and required command metadata
    /// before allowing the reactivation mutation to execute.
    ///
    /// Non-reactivation mutations are automatically allowed because this policy
    /// only applies to <see cref="ReactivateWalletMutation"/>.
    /// </remarks>
    public PolicyDecision Evaluate(IMutation<Wallet> mutation, Wallet state)
    {
        return mutation switch
        {
            not ReactivateWalletMutation => PolicyDecision.Allow(Name),
            ReactivateWalletMutation reactivate => state.Status switch
            {
                WalletStatus.Active => PolicyDecision.Deny("Wallet is already active", Name),
                WalletStatus.Closed => PolicyDecision.DenyCritical("Closed wallet cannot be reactivated", Name),
                not WalletStatus.Suspended => PolicyDecision.Deny("Only suspended wallets can be reactivated", Name),
                _ => string.IsNullOrWhiteSpace(reactivate.Context.Reason)
                    ? PolicyDecision.Deny("Reactivation reason is required", Name)
                    : PolicyDecision.Allow(Name)
            }
        };
    }
}
