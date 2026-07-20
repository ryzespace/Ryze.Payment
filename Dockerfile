FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Ryze.Payment.slnx", "."]

COPY ["Ryze.Host/Ryze.Host.csproj", "Ryze.Host/"]
COPY ["Ryze.Application/Ryze.Application.csproj", "Ryze.Application/"]
COPY ["Ryze.Domain/Ryze.Domain.csproj", "Ryze.Domain/"]
COPY ["Ryze.Infrastructure/Ryze.Infrastructure.csproj", "Ryze.Infrastructure/"]

RUN dotnet restore "Ryze.Payment.slnx"

COPY . .

WORKDIR "/src/Ryze.Host"
RUN dotnet build "Ryze.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ryze.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ryze.Host.dll"]
