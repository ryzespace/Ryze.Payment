using Ryze.Application.Features.Walllet.Contexts;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Maps wallet state-change gRPC requests into application contexts.
/// </summary>
/// <remarks>
/// Provides mapping for suspend and reactivate operations,
/// normalizing invalid identifiers and null reasons to safe defaults.
/// </remarks>
public static class WalletStateChangeMapping
{
    /// <summary>
    /// Maps <see cref="SuspendWalletRequest"/> into <see cref="WalletSuspendContext"/>.
    /// </summary>
    /// <param name="request">Incoming gRPC suspend-wallet request.</param>
    /// <returns>Application context used by suspend command handlers.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    public static WalletSuspendContext ToContext(SuspendWalletRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new()
        {
            WalletId = Guid.TryParse(request.WalletId, out var id) ? id : Guid.Empty,
            Reason = request.Reason ?? string.Empty
        };
    }

    /// <summary>
    /// Maps <see cref="ReactivateWalletRequest"/> into <see cref="WalletReactivateContext"/>.
    /// </summary>
    /// <param name="request">Incoming gRPC reactivate-wallet request.</param>
    /// <returns>Application context used by reactivate command handlers.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    public static WalletReactivateContext ToContext(ReactivateWalletRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new WalletReactivateContext
        {
            WalletId = Guid.TryParse(request.WalletId, out var id) ? id : Guid.Empty,
            Reason = request.Reason ?? string.Empty
        };
    }
}
