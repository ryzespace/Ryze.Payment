using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Features.WalletBalance.Contexts;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Requests;

/// <summary>
/// Command representing a request to create a new wallet in the system.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains the necessary data to create a wallet: owners, initial balance, wallet type, currency, and idempotency key.</item>
/// <item>Is immutable and can be safely passed to handlers or the message bus.</item>
/// <item>Encapsulates <see cref="WalletCreationContext"/> for context-aware operations.</item>
/// </list>
/// </remarks>
/// <param name="Request">Ambient request metadata and correlation context.</param>
/// <param name="Context">Domain context containing wallet creation details.</param>
public sealed record CreateWalletCommand(
    RequestContext Request,
    WalletCreationContext Context
);
