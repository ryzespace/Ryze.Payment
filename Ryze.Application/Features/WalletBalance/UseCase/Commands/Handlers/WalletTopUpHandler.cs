using Ryze.Application.Features.WalletBalance.UseCase.Commands.Requests;
using Ryze.Domain.Features.WalletBalance.Providers;

namespace Ryze.Application.Features.WalletBalance.UseCase.Commands.Handlers;

/// <summary>
/// Handler responsible for processing wallet top-up commands.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Receives a <see cref="WalletTopUpCommand"/> containing top-up details.</item>
/// <item>Selects the appropriate <see cref="ITopUpProvider"/> based on <see cref="WalletTopUpCommand.Provider"/>.</item>
/// <item>Invokes the top-up operation asynchronously via the selected provider.</item>
/// <item>Throws an <see cref="InvalidOperationException"/> if no matching provider is found.</item>
/// </list>
/// </remarks>
public class WalletTopUpHandler
{
    /// <summary>
    /// Handles the wallet top-up command using available top-up providers.
    /// </summary>
    /// <param name="message">The top-up command containing amount and provider information.</param>
    /// <param name="providers">The collection of available <see cref="ITopUpProvider"/> implementations.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no provider matches the command's provider type.</exception>
    public async ValueTask Handle(
        WalletTopUpCommand message,
        IEnumerable<ITopUpProvider> providers)
    {
        var provider = providers.FirstOrDefault(p => p.ProviderType == message.Provider)
                       ?? throw new InvalidOperationException("Unknown provider");

        await provider.TopUp(message.Amount);
    } 
}