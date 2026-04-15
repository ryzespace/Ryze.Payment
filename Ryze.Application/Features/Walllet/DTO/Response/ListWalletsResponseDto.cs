namespace Ryze.Application.Features.Walllet.DTO.Response;

/// <summary>
/// Response DTO for wallet listing operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains current page items after applying filters and projection rules.</item>
/// <item>Includes continuation metadata for token-based pagination flows.</item>
/// <item>Exposes total matched count to support client-side paging indicators.</item>
/// </list>
/// </remarks>
public sealed class ListWalletsResponseDto
{
    /// <summary>
    /// Wallet items included in the current result page.
    /// </summary>
    /// <remarks>
    /// Collection may be empty when no wallets match filters or the requested page is beyond available data.
    /// </remarks>
    public IReadOnlyCollection<WalletDto> Wallets { get; init; } = [];

    /// <summary>
    /// Opaque token used to request the next page; <see langword="null"/> when no next page exists.
    /// </summary>
    /// <remarks>
    /// Clients should pass this value unchanged to subsequent list requests.
    /// </remarks>
    public string? NextPageToken { get; init; }

    /// <summary>
    /// Total number of wallets that match the applied filters.
    /// </summary>
    /// <remarks>
    /// Represents total matches before page slicing and can be used for UI pagination controls.
    /// </remarks>
    public int TotalCount { get; init; }
}
