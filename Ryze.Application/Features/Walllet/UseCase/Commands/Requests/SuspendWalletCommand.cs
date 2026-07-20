using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Requests;

/// <summary>
/// Command representing a request to suspend an existing wallet.
/// </summary>
/// <param name="Request">Ambient request metadata and correlation context.</param>
/// <param name="SuspendContext">Domain context containing wallet suspension details.</param>
public sealed record SuspendWalletCommand(
    RequestContext Request,
    WalletSuspendContext SuspendContext
);
