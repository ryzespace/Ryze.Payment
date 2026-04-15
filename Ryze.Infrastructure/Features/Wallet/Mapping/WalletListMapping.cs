using Mapster;
using Payment.Common.Grpc;
using Ryze.Application.Features.Walllet.Contexts.Getters;
using Ryze.Application.Features.Walllet.DTO.Response;
using RyzeSpace.Wallet.Contracts.V1;

namespace Ryze.Infrastructure.Features.Wallet.Mapping;

/// <summary>
/// Configures Mapster mappings for wallet list (pagination) operations.
/// </summary>
/// <remarks>
/// Handles conversion of paginated wallet queries and responses between:
/// - gRPC request models<br/>
/// - application list contexts<br/>
/// - response DTOs and gRPC contracts<br/><br/>
///
/// Includes normalization of pagination parameters, optional filters,
/// and safe handling of unspecified enum values.
/// </remarks>
public static class WalletListMapping
{
    /// <summary>
    /// Registers Mapster configuration for wallet list requests and responses.
    /// </summary>
    /// <remarks>
    /// Request mapping:
    /// - Normalizes <c>PageSize</c> to default value (50) when invalid<br/>
    /// - Converts empty strings to <see langword="null"/> for optional filters<br/>
    /// - Translates <c>Unspecified</c> enums into nullable values<br/>
    /// - Converts Protobuf timestamps into <see cref="DateTimeOffset"/>
    /// <br/><br/>
    ///
    /// Response mapping:
    /// - Maps wallet collections into gRPC contract types<br/>
    /// - Ensures safe defaults for pagination metadata (e.g., <c>NextPageToken</c>)<br/>
    /// - Preserves total count for client-side pagination
    /// </remarks>
    public static void RegisterMappings()
    {
        TypeAdapterConfig<ListWalletsRequest, WalletListContext>
            .NewConfig()
            .Map(dest => dest.PageSize, src => src.PageSize > 0 ? src.PageSize : 50)
            .Map(dest => dest.PageToken, src => string.IsNullOrWhiteSpace(src.PageToken) ? null : src.PageToken)
            .Map(dest => dest.OwnerId, src => string.IsNullOrWhiteSpace(src.OwnerId) ? null : src.OwnerId)
            .Map(dest => dest.Status, src => src.Status == WalletStatus.Unspecified ? (WalletStatus?)null : src.Status)
            .Map(dest => dest.WalletType, src => src.WalletType == WalletType.Unspecified ? (WalletType?)null : src.WalletType)
            .Map(dest => dest.Tags, src => src.Tags)
            .Map(dest => dest.CreatedAfter,
                src => src.CreatedAfter.ToDateTimeOffset())
            .Map(dest => dest.CreatedBefore,
                src => src.CreatedBefore.ToDateTimeOffset());

        TypeAdapterConfig<ListWalletsResponseDto, ListWalletsResponse>
            .NewConfig()
            .AfterMapping((src, dest) =>
            {
                if (src.Wallets.Count > 0)
                {
                    dest.Wallets.AddRange(
                        src.Wallets.Adapt<RyzeSpace.Wallet.Contracts.V1.Wallet[]>()
                    );
                }

                dest.NextPageToken = src.NextPageToken ?? string.Empty;
                dest.TotalCount = src.TotalCount;
            });
    }
}