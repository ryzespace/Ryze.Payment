using Mapster;
using Ryze.Application.Features.WalletBalance.DTO;
using Ryze.Application.Features.Walllet.DTO;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using AddressEntity = Ryze.Domain.Features.Wallet.Entity.Address;
using MoneyValue = Ryze.Domain.Features.Wallet.ValueObject.Money;
using WalletEntity = Ryze.Domain.Features.Wallet.Entity.Wallet;
using WalletOwnerEntity = Ryze.Domain.Features.Wallet.Entity.WalletOwner;

namespace Ryze.Application.Features.Walllet.Mapping;

/// <summary>
/// Central Mapster configuration for wallet query projections.
/// Registers conversions between wallet domain entities and application DTOs,
/// including value objects, nested types, and UTC timestamp normalization.
/// </summary>
public static class WalletQueryMapping
{
    private static readonly TypeAdapterConfig Config = BuildConfig();

    /// <summary>
    /// Creates and initializes the shared mapping configuration.
    /// The mappings are also registered into <see cref="TypeAdapterConfig.GlobalSettings"/>
    /// so they can be reused throughout the application.
    /// </summary>
    private static TypeAdapterConfig BuildConfig()
    {
        RegisterInto(TypeAdapterConfig.GlobalSettings);
        return TypeAdapterConfig.GlobalSettings;
    }

    /// <summary>
    /// Projects a wallet aggregate into a query DTO.
    /// </summary>
    /// <remarks>
    /// Owners and balance are included only when explicitly requested,
    /// avoiding unnecessary object graph traversal and mapping work.
    /// </remarks>
    public static WalletDto ToDto(WalletEntity wallet, bool includeOwners, bool includeBalance)
    {
        var projection = new WalletProjection(wallet, includeOwners, includeBalance);
        var dto = projection.Adapt<WalletDto>(Config);

        if (includeOwners)
            dto.Owners = wallet.Owners.Select(o => o.Adapt<WalletOwnerDto>(Config)).ToList();

        return dto;
    }

    /// <summary>
    /// Projects a wallet aggregate using flags from the supplied query context.
    /// </summary>
    public static WalletDto ToDto(WalletEntity wallet, WalletGetContext context)
    {
        return ToDto(wallet, context.IncludeOwners, context.IncludeBalance);
    }

    public static void RegisterMappings() => RegisterInto(TypeAdapterConfig.GlobalSettings);

    private static void RegisterInto(TypeAdapterConfig config)
    {

        config.NewConfig<DateTime, DateTimeOffset>()
            .MapWith(src =>
                new DateTimeOffset(
                    src.Kind == DateTimeKind.Utc
                        ? src
                        : src.Kind == DateTimeKind.Local
                            ? src.ToUniversalTime()
                            : DateTime.SpecifyKind(src, DateTimeKind.Utc)));

        config.NewConfig<DateTime?, DateTimeOffset?>()
            .MapWith(src => src.HasValue ? ToUtcOffset(src.Value) : null);

        config.NewConfig<MoneyValue, MoneyDto>();

        config.NewConfig<AddressEntity, AddressDto>()
            .Map(dest => dest.Street, src => src.Street.Value)
            .Map(dest => dest.StreetTwo, src => src.StreetTwo ?? default!)
            .Map(dest => dest.City, src => src.City.Value)
            .Map(dest => dest.State, src => src.State ?? default!)
            .Map(dest => dest.PostalCode, src => src.PostalCode.Value)
            .Map(dest => dest.Country, src => src.Country.Value);

        config.NewConfig<WalletOwnerEntity, WalletOwnerDto>()
            .Map(dest => dest.NationalId, src => src.NationalId ?? default!)
            .Map(dest => dest.Metadata, src => src.Metadata.Values)
            .Map(dest => dest.Address, src => new AddressDto
            {
                Street = src.Address.Street.Value,
                StreetTwo = src.Address.StreetTwo ?? default!,
                City = src.Address.City.Value,
                State = src.Address.State ?? default!,
                PostalCode = src.Address.PostalCode.Value,
                Country = src.Address.Country.Value,
            });

        config.NewConfig<WalletProjection, WalletDto>()
            .Map(dest => dest.WalletId, src => src.Wallet.Id.ToString())
            .Map(dest => dest.WalletType, src => src.Wallet.Type)
            .Map(dest => dest.Currency, src => src.Wallet.Currency)
            .Map(dest => dest.Status, src => src.Wallet.Status)
            .Map(dest => dest.CreatedAt, src => src.Wallet.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.Wallet.UpdatedAt)
            .Map(dest => dest.ClosedAt, src => src.Wallet.ClosedAt)
            .Map(dest => dest.Metadata, src => src.Wallet.Metadata.Values)
            .Map(dest => dest.AccountNumber,
                src => src.Wallet.AccountNumber ?? default!)
            .Map(dest => dest.RoutingNumber,
                src => src.Wallet.RoutingNumber ?? default!)
            .Map(dest => dest.Tags, src => src.Wallet.Tags.Values.ToList());
    }

    // <summary>
    /// Lightweight projection carrying conditional mapping flags together with
    /// the wallet aggregate.
    /// </summary>
    private sealed record WalletProjection(WalletEntity Wallet, bool IncludeOwners, bool IncludeBalance);

    /// <summary>
    /// Normalizes a <see cref="DateTime"/> to a UTC
    /// <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTimeKind.Unspecified"/> values are treated as UTC
    /// to provide deterministic serialization across service boundaries.
    /// </remarks>
    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTimeOffset(utc);
    }
}
