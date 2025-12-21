using Ryze.Application.Features.WalletBalance.UseCase.Commands.Handlers;
using Ryze.Domain.Features.WalletBalance;

namespace Ryze.Application.Features.WalletBalance.UseCase.Commands.Requests;

/// <summary>
/// Command representing a request to top up a wallet balance.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates the amount to be added to the wallet.</item>
/// <item>Specifies the <see cref="PaymentProvider"/> used for the top-up.</item>
/// <item>Used by <see cref="WalletTopUpHandler"/> to execute the top-up operation.</item>
/// </list>
/// </remarks>
/// <param name="Amount">The amount of money to top up.</param>
/// <param name="Provider">The payment provider to process the top-up.</param>
public sealed record WalletTopUpCommand(
    decimal Amount,
    PaymentProvider Provider
);