using Ryze.Host.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApp(builder.Configuration)
    .AddGrpcServices();

builder.Host.ConfigureWolverine(builder.Configuration);

var app = builder.Build();

app.MapGrpcEndpoints()
    .Run();