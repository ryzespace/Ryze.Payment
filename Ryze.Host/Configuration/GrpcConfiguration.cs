using Ryze.Infrastructure.Features.WalletBalance.Processors.Processor;

namespace Ryze.Host.Configuration;

/// <summary>
/// Provides extension methods to configure gRPC services and endpoints for the application.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates registration of gRPC services into the <see cref="IServiceCollection"/>.</item>
/// <item>Maps gRPC endpoints to the <see cref="WebApplication"/> instance.</item>
/// <item>Includes a default HTTP GET endpoint to provide guidance on using gRPC clients.</item>
/// </list>
/// </remarks>
public static class GrpcConfiguration
{
    public static void AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();
    }

    public static WebApplication MapGrpcEndpoints(this WebApplication app)
    {
        app.MapGrpcService<WalletBalanceManagementProcessor>();
        
        app.MapGet("/", () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        return app;
    }
}