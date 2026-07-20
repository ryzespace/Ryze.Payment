using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using Ryze.Application.Features.Walllet.Mutations.Creation;
using Ryze.Domain.Features.Wallet.Entity;
using Ryze.Domain.Features.Wallet.Repositories;

namespace Ryze.Application.Features.Walllet.Policies.Creation;

/// <summary>
/// Mutation policy enforcing wallet ownership limits during wallet creation.
/// </summary>
/// <remarks>
/// Prevents excessive wallet creation for a single owner by validating the
/// number of existing wallets associated with each requested owner.
///
/// This policy protects against uncontrolled wallet proliferation and enforces
/// application-level ownership constraints before the creation mutation is
/// executed.
///
/// The policy only applies to <see cref="CreateWalletMutation"/> operations
/// and does not modify aggregate state.
/// </remarks>
public sealed class WalletOwnerLimitPolicy(IWalletRepository walletRepository) : IMutationPolicy<Wallet>
{
    public string Name => "wallet.owner.limit";
    public int Priority => 100;
    public string Description => "Limits the number of wallets an owner can have";

    /// <summary>
    /// Maximum number of wallets allowed per owner.
    /// </summary>
    private const int MaxWalletsPerOwner = 3;

    /// <summary>
    /// Evaluates whether wallet creation satisfies owner wallet count limits.
    /// </summary>
    /// <remarks>
    /// Retrieves the current wallet count for each requested owner and denies
    /// creation when any owner exceeds the configured ownership limit.
    ///
    /// Non-wallet-creation mutations are automatically allowed because this
    /// policy only applies to wallet creation workflows.
    /// </remarks>
    /// <param name="mutation">
    /// Mutation being evaluated.
    /// </param>
    /// <param name="state">
    /// Current wallet aggregate state.
    /// </param>
    /// <returns>
    /// Policy decision indicating whether the mutation can proceed.
    /// </returns>
    public PolicyDecision Evaluate(IMutation<Wallet> mutation, Wallet state)
    {
        if (mutation is not CreateWalletMutation create) return PolicyDecision.Allow(Name);

        foreach (var owner in create.Input.Owners)
        {
            var count = walletRepository.CountWalletsByOwner(owner.OwnerId);

            if (count >= MaxWalletsPerOwner)
                return PolicyDecision.Deny(
                    $"Owner {owner.OwnerId} already has {MaxWalletsPerOwner} wallets.",
                    Name);
        }

        return PolicyDecision.Allow(Name);
    }
}
