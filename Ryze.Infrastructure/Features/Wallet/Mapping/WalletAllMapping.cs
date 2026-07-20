using Google.Protobuf.WellKnownTypes;
using Mapster;
using Ryze.Application.Features.WalletBalance.DTO;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Application.Features.Walllet.Mapping;
using Ryze.Application.Features.Walllet.DTO.Response;
using Ryze.Infrastructure.Features.WalletBalance.Mapping;
using RyzeSpace.Wallet.Contracts.V1;
using Proto = Payment.Common.Grpc;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Aggregates and registers all Mapster mappings used in the Wallet module.
/// </summary>
/// <remarks>
/// Centralized mapping configuration for:
/// - Wallet commands and queries<br/>
/// - Wallet owners and balances<br/>
/// - gRPC contract transformations<br/>
/// - Shared value object conversions (Money, Address, Balance)<br/><br/>
///
/// This class acts as a composition root for Mapster configuration in the Wallet bounded context.
/// It ensures all mappings are registered in a deterministic order before compilation.
/// </remarks>
public static class WalletAllMapping
{
    /// <summary>
    /// Registers all Wallet-related Mapster mappings and compiles global configuration.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// - Feature-level mappings (Wallet, Owner, Requests, Lists)<br/>
    /// - Global type conversions (DTO ↔ gRPC contracts)<br/>
    /// - Protobuf timestamp conversions<br/>
    /// - Collection and metadata mapping strategies<br/><br/>
    ///
    /// Finally calls <see cref="TypeAdapterConfig.Compile()"/> to optimize mapping performance.
    /// </remarks>
    public static void RegisterMappings()
    {
        WalletMapping.RegisterMappings();
        WalletRequestMapping.RegisterMappings();
        WalletOwnerMapping.RegisterMappings();
        GetWalletMapping.RegisterMappings();
        WalletListMapping.RegisterMappings();
        WalletQueryMapping.RegisterMappings();
        WalletBalanceMapping.RegisterMappings();

        var global = TypeAdapterConfig.GlobalSettings;

        global.NewConfig<DateTimeOffset?, Timestamp?>()
              .MapWith(src => src.HasValue ? Timestamp.FromDateTime(src.Value.UtcDateTime) : null);

        global.NewConfig<AddressDto, Address>()
              .Map(dest => dest.StreetLine1, src => src.Street)
              .Map(dest => dest.StreetLine2, src => src.StreetTwo ?? string.Empty)
              .Map(dest => dest.City, src => src.City)
              .Map(dest => dest.StateProvince, src => src.State ?? string.Empty)
              .Map(dest => dest.PostalCode, src => src.PostalCode)
              .Map(dest => dest.CountryCode, src => src.Country);

        global.NewConfig<MoneyDto, Money>()
              .Map(dest => dest.Currency, src => (Proto.Currency)src.Currency);

        global.NewConfig<BalanceDto, Balance>();

        global.NewConfig<WalletOwnerDto, WalletOwner>()
              .Map(dest => dest.OwnerId, src => src.OwnerId)
              .Map(dest => dest.NationalId, src => src.NationalId ?? string.Empty)
              .Map(dest => dest.Role, src => (Proto.OwnerRole)src.Role)
              .Map(dest => dest.DateOfBirth, src => src.DateOfBirth.HasValue
                  ? Timestamp.FromDateTimeOffset(src.DateOfBirth.Value)
                  : null)
              .Map(dest => dest.AddedAt, src => Timestamp.FromDateTimeOffset(src.AddedAt))
              .AfterMapping((src, dest) =>
              {
                  dest.Address = src.Address?.Adapt<Address>() ?? new Address();

                  if (src.Metadata != null)
                      foreach (var kvp in src.Metadata)
                          dest.Metadata[kvp.Key] = kvp.Value;
              });

        global.NewConfig<WalletDto, RyzeSpace.Wallet.Contracts.V1.Wallet>()
              .Map(dest => dest.WalletId, src => src.WalletId)
              .Map(dest => dest.AccountNumber, src => src.AccountNumber ?? string.Empty)
              .Map(dest => dest.RoutingNumber, src => src.RoutingNumber ?? string.Empty)
              .Map(dest => dest.WalletType, src => (Proto.WalletType)src.WalletType)
              .Map(dest => dest.Currency, src => (Proto.Currency)src.Currency)
              .Map(dest => dest.Status, src => (Proto.WalletStatus)src.Status)
              .Map(dest => dest.CreatedAt, src => Timestamp.FromDateTimeOffset(src.CreatedAt))
              .Map(dest => dest.UpdatedAt, src => src.UpdatedAt.HasValue
                  ? Timestamp.FromDateTimeOffset(src.UpdatedAt.Value)
                  : null)
              .Map(dest => dest.ClosedAt, src => src.ClosedAt.HasValue
                  ? Timestamp.FromDateTimeOffset(src.ClosedAt.Value)
                  : null)
              .AfterMapping((src, dest) =>
              {
                  if (src.Balance != null)
                      dest.Balance = src.Balance.Adapt<Balance>();

                  if (src.Owners != null)
                      dest.Owners.AddRange(src.Owners.Adapt<WalletOwner[]>());

                  if (src.Metadata != null)
                      foreach (var kvp in src.Metadata)
                          dest.Metadata[kvp.Key] = kvp.Value;

                  if (src.Tags != null)
                      dest.Tags.AddRange(src.Tags);
              });

        global.NewConfig<GetWalletResponseDto, GetWalletResponse>()
              .Map(dest => dest.Wallet, src => src.Wallet.Adapt<RyzeSpace.Wallet.Contracts.V1.Wallet>());

        global.NewConfig<CreateWalletResponseDto, CreateWalletResponse>()
              .Map(dest => dest.Status, src => (Proto.WalletStatus)src.Status);

        global.Compile();
    }
}