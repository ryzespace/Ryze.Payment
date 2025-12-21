using Ryze.Application.Features.WalletBalance.UseCase.Commands.Handlers;
using Wolverine;

namespace Ryze.Host.Configuration.Factory;

/// <summary>
/// Factory class responsible for registering event handlers with Wolverine.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Includes assemblies containing command and event handlers.</item>
/// <item>Enables Wolverine to discover and wire up handlers automatically.</item>
/// <item>Centralizes handler registration to simplify maintenance and discovery.</item>
/// </list>
/// </remarks>
public static class EventHandlerRegistrationFactory
{
    public static void IncludeEventHandlers(this WolverineOptions opts)
    {
        opts.Discovery.IncludeAssembly(typeof(WalletTopUpHandler).Assembly);
    }
}