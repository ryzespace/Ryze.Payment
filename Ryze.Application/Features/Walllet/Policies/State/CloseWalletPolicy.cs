using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Walllet.Mutations.State;
using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.Policies.State;

/// <summary>
/// Mutation policy validating whether a wallet can transition into a closed state.
/// </summary>
/// <remarks>
/// Protects the wallet lifecycle by enforcing application-level rules before
/// executing <see cref="CloseWalletMutation"/>.
///
/// The policy prevents invalid closure attempts, including closing wallets that
/// are already closed or wallets currently blocked by a frozen state.
///
/// A closure reason is required to ensure the lifecycle transition has an
/// explicit business justification for audit and compliance purposes.
///
/// This policy only evaluates wallet close mutations and does not modify the
/// wallet aggregate state.
/// </remarks>
public sealed class CloseWalletPolicy : IMutationPolicy<Wallet>
{
    public string Name => "wallet.close";
    public int Priority => 100;
    public string Description => "Validates whether a wallet can be closed";

    /// <summary>
    /// Evaluates whether the wallet closure mutation is allowed.
    /// </summary>
    /// <remarks>
    /// Checks wallet lifecycle state and required closure metadata before
    /// allowing the mutation to proceed.
    ///
    /// Non-close mutations are automatically allowed because this policy only
    /// applies to <see cref="CloseWalletMutation"/>.
    /// </remarks>
    public PolicyDecision Evaluate(IMutation<Wallet> mutation, Wallet state)
    {
        return mutation switch
        {
            not CloseWalletMutation => PolicyDecision.Allow(Name),
            CloseWalletMutation close => state.Status switch
            {
                WalletStatus.Closed => PolicyDecision.Deny("Wallet is already closed", Name),
                WalletStatus.Frozen => PolicyDecision.Deny("Frozen wallet cannot be closed", Name),
                _ => string.IsNullOrWhiteSpace(close.Context.Reason)
                    ? PolicyDecision.Deny("Closure reason is required", Name)
                    : PolicyDecision.Allow(Name)
            }
        };
    }
}
