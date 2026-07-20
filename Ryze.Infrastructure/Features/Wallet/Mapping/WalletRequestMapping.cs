using Mapster;
using Ryze.Application.Features.Walllet.Contexts;
using Ryze.Domain.Shared.Enum;
using RyzeSpace.Wallet.Contracts.V1;
using WalletTypeDomain = Ryze.Domain.Shared.Enum.WalletType;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Configures Mapster mappings for wallet creation requests.
/// </summary>
/// <remarks>
/// Handles transformation of gRPC wallet creation requests into application contexts,
/// including enum parsing, nested owner mapping, and direct passthrough of metadata.
/// </remarks>
public static class WalletRequestMapping
{
    /// <summary>
    /// Registers Mapster configuration for <see cref="CreateWalletRequest"/>
    /// to <see cref="WalletCreationContext"/> mapping.
    /// </summary>
    /// <remarks>
    /// Mapping behavior includes:
    /// - Conversion of gRPC currency enum to domain <see cref="Currency"/>
    /// - Parsing of wallet type string into <see cref="WalletTypeDomain"/>
    /// - Adaptation of nested owners into <see cref="WalletOwnerContext"/> list
    /// - Direct mapping of metadata and tags collections
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// May be thrown if nested collections (e.g. <c>Owners</c>) are null depending on Mapster configuration.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <c>WalletType</c> cannot be parsed into <see cref="WalletTypeDomain"/>.
    /// </exception>
    public static void RegisterMappings()
    {
        TypeAdapterConfig<CreateWalletRequest, WalletCreationContext>
            .NewConfig()
            .Map(dest => dest.CurrencyCode, src => (Currency)src.Currency)
            .Map(dest => dest.WalletType, src => Enum.Parse<WalletTypeDomain>(src.WalletType, true))
            .Map(dest => dest.Owners, src => src.Owners.Adapt<List<WalletOwnerContext>>())
            .Map(dest => dest.Metadata, src => src.Metadata)
            .Map(dest => dest.Tags, src => src.Tags);
    }
}