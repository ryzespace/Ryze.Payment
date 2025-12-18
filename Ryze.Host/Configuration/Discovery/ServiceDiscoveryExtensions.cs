using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ryze.Host.Configuration.Discovery;

/// <summary>
/// Provides extension methods for automatic discovery and registration of services into the DI container.
/// </summary>
public static class ServiceDiscoveryExtensions
{
    /// <summary>
    /// Scans the specified assemblies for service classes according to <see cref="ServiceDiscoveryOptions"/>
    /// and registers them into the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register discovered services into.</param>
    /// <param name="assemblies">Assemblies to scan for service classes.</param>
    /// <param name="configure"></param>
    /// <param name="logger"></param>
    /// <returns>The original <see cref="IServiceCollection"/> to allow method chaining.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Filters types based on allowed or excluded namespaces and types defined in <see cref="ServiceDiscoveryOptions"/>.</item>
    /// <item>Skips registration of interfaces or exception types if specified in <see cref="ServiceDiscoveryOptions.SkipInterfaces"/> 
    /// or <see cref="ServiceDiscoveryOptions.SkipExceptions"/>.</item>
    /// <item>Registers discovered concrete classes as all of their implemented interfaces with the configured lifetime.</item>
    /// <item>Automatically adds a <see cref="ServiceDiscoveryExtensions"/> to trigger registration during app startup.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddDiscoveredServices(
        this IServiceCollection services,
        Assembly[] assemblies,
        Action<ServiceDiscoveryOptions>? configure = null,
        ILogger? logger = null)
    {
        var opts = new ServiceDiscoveryOptions();
        configure?.Invoke(opts);

        logger ??= NullLogger.Instance;

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .Where(type => ServiceDiscoveryFilter.IsValidServiceType(type, logger, opts)))
            .AsImplementedInterfaces()
            .WithLifetime(opts.Lifetime)
        );

        return services;
    }

}