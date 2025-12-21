using Ryze.Host.Configuration.Factory;
using Wolverine;

namespace Ryze.Host.Configuration;

public static class InfrastructureConfiguration
{
    public static IServiceCollection ConfigureMarten(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }

    public static void ConfigureWolverine(
        this ConfigureHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        hostBuilder.UseWolverine(opts =>
        {
           // opts.UseFluentValidation();
           opts.IncludeEventHandlers();
        });
    }
}