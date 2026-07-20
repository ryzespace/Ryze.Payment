using ModularityKit.Context.Abstractions;
using Ryze.Application.Features.Walllet.DTO.Response;

namespace Ryze.Application.Features.Walllet.Interfaces;

/// <summary>
/// Write-only application service responsible for mutating wallet state.
/// The execution context (tenant, identity, permissions, etc.) is
/// resolved from <see cref="IContextAccessor{TContext}"/> and is expected to
/// be established by the caller before invoking any operation.
/// </summary>
public interface IWriteWalletService
{
    /// <summary>
    /// Creates a new wallet using the current
    /// <c>WalletCreateContext</c>.
    /// </summary>
    /// <returns>
    /// Information describing the newly created wallet.
    /// </returns>
    Task<CreateWalletResponseDto> CreateWallet();

    /// <summary>
    /// Reactivates the wallet identified by the current
    /// <c>WalletReactivateContext</c>.
    /// </summary>
    /// <remarks>
    /// Restores a previously suspended wallet to an operational state,
    /// allowing it to participate in later wallet operations.
    /// Throws if the wallet cannot be reactivated because of its current
    /// state or business rules.
    /// </remarks>
    Task ReactivateWallet();

    /// <summary>
    /// Suspends the wallet identified by the current
    /// <c>WalletSuspendContext</c>.
    /// </summary>
    /// <remarks>
    /// Prevents the wallet from participating in new operations while
    /// preserving its existing state and history. Throws if the wallet
    /// is already suspended or cannot be suspended because of business
    /// constraints.
    /// </remarks>
    Task SuspendWallet();
}