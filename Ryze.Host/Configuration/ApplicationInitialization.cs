using System.Reflection;
using Ryze.Domain.Features.WalletBalance;
using Ryze.Host.Configuration.Discovery;
using Ryze.Infrastructure.Features.WalletBalance.Processors.Interfaces;

namespace Ryze.Host.Configuration;

/// <summary>
/// Provides application-level initialization and configuration helpers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Registers services discovered across relevant assemblies.</item>
/// <item>Configures logging for service discovery.</item>
/// <item>Encapsulates host, domain, and infrastructure assembly references for discovery.</item>
/// </list>
/// </remarks>
public static class ApplicationInitialization
{
    public static IServiceCollection ConfigureApp(
        this IServiceCollection services,
        IConfiguration configuration)
    { 
        var discoveryLogger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        }).CreateLogger("ServiceDiscovery");

        services.AddDiscoveredServices(GetRelevantAssemblies(), opts =>
        {
            configuration.GetSection("ServiceDiscovery").Bind(opts);
        }, discoveryLogger);
        
        return services;
    }
    
    #region Private Helpers

    private static Assembly[] GetRelevantAssemblies() =>
        new[]
        {
            typeof(ApplicationInitialization).Assembly,  // Host
            typeof(PaymentProvider).Assembly,               // Domain
            typeof(IWalletBalanceWriteOperations).Assembly,            // Infrastructure
        }.Distinct().ToArray();
    #endregion
}