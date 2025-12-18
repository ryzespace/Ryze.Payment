using Ryze.Host.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApp(builder.Configuration)
    .AddGrpcServices();

var app = builder.Build();

app.MapGrpcEndpoints()
    .Run();