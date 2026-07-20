using Ryze.Application.Features.WalletBalance.DTO;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Walllet.DTO;

/// <summary>
/// Data Transfer Object representing a wallet with its owners, balances, and metadata.
/// </summary>
/// <remarks>
/// <para>
/// This DTO is used for interlayer communication (e.g., application → gRPC or REST transport)
/// and mirrors the essential properties of a <c>Wallet</c> entity.
/// </para>
/// <para>
/// Includes wallet identity, owners, financial information, status, timestamps, metadata, and tags.
/// All properties are intended to be read-only from the consumer perspective.
/// </para>
/// </remarks>
public sealed class WalletDto
{
    /// <summary>
    /// Unique identifier of the wallet.
    /// </summary>
    public string WalletId { get; init; } = null!;

    /// <summary>
    /// List of wallet owners associated with this wallet.
    /// </summary>
    public IReadOnlyList<WalletOwnerDto>? Owners { get; set; }

    /// <summary>
    /// Optional balance information of the wallet.
    /// </summary>
    public BalanceDto? Balance { get; set; }

    /// <summary>
    /// Type of the wallet (e.g., personal, business).
    /// </summary>
    public WalletType WalletType { get; init; }

    /// <summary>
    /// Currency of the wallet balance.
    /// </summary>
    public Currency Currency { get; init; }

    /// <summary>
    /// Current status of the wallet (e.g., Active, Suspended, Closed).
    /// </summary>
    public WalletStatus Status { get; init; }

    /// <summary>
    /// Timestamp when the wallet was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Optional timestamp when the wallet was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Optional timestamp when the wallet was closed.
    /// </summary>
    public DateTimeOffset? ClosedAt { get; init; }

    /// <summary>
    /// Optional metadata associated with the wallet.
    /// Keys are expected to be normalized (lowercase, trimmed).
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Optional bank account number associated with the wallet.
    /// </summary>
    public string? AccountNumber { get; init; }

    /// <summary>
    /// Optional routing number associated with the wallet.
    /// </summary>
    public string? RoutingNumber { get; init; }

    /// <summary>
    /// Optional list of tags associated with the wallet.
    /// Tags are expected to be normalized (lowercase, trimmed).
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }
}
