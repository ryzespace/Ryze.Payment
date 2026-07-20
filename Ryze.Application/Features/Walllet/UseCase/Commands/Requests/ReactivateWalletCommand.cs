using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Requests;

/// <summary>
/// Command representing a request to reactivate an existing wallet.
/// </summary>
/// <param name="Request">Ambient request metadata and correlation context.</param>
/// <param name="ReactivateContext">Domain context containing wallet reactivation details.</param>
public sealed record ReactivateWalletCommand(
    RequestContext Request,
    WalletReactivateContext ReactivateContext
);
