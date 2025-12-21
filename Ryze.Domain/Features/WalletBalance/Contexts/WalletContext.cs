using ModularityKit.Context.Abstractions;

namespace Ryze.Domain.Features.WalletBalance.Contexts;

/// <summary>
/// Represents the context of operations performed on a specific wallet.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks metadata of a wallet operation, including <see cref="WalletId"/> and <see cref="IdempotencyKey"/>.</item>
/// <item>Stores information about the last top-up, including amount, provider, and timestamp.</item>
/// <item>Provides a factory method <see cref="Create"/> for generating contexts with unique IDs.</item>
/// <item>Supports updating the last top-up data via <see cref="SetLastTopUp(decimal, PaymentProvider)"/>.</item>
/// </list>
/// </remarks>
public sealed class WalletContext : IContext
{
    /// <summary>
    /// Unique identifier of the wallet context.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Timestamp when the context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier of the wallet associated with this context.
    /// </summary>
    public Guid WalletId { get; }

    /// <summary>
    /// Idempotency key for ensuring operation uniqueness.
    /// </summary>
    public string IdempotencyKey { get; }

    /// <summary>
    /// Amount of the last top-up performed on the wallet.
    /// </summary>
    public decimal LastTopUpAmount { get; private set; }

    /// <summary>
    /// Payment provider used for the last top-up.
    /// </summary>
    public PaymentProvider? LastTopUpProvider { get; private set; }

    /// <summary>
    /// Timestamp of the last top-up operation.
    /// </summary>
    public DateTimeOffset? LastTopUpAt { get; private set; }
    private WalletContext(
        string id,
        Guid walletId,
        string idempotencyKey)
    {
        //if (walletId == Guid.Empty) throw new ArgumentException("WalletId cannot be empty", nameof(walletId));
       // if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("IdempotencyKey cannot be empty", nameof(idempotencyKey));
       
        Id = id;
        WalletId = walletId;
        IdempotencyKey = idempotencyKey;
    }

    public static WalletContext Create(
        Guid walletId,
        string idempotencyKey)
    {
        return new WalletContext(
            Guid.NewGuid().ToString("N"),
            walletId,
            idempotencyKey
        );
    }
    
    /// <summary>
    /// Updates the last top-up information for this wallet.
    /// </summary>
    /// <param name="amount">Amount of the top-up.</param>
    /// <param name="provider">Payment provider used for the top-up.</param>
    public void SetLastTopUp(decimal amount, PaymentProvider provider)
    {
        LastTopUpAmount = amount;
        LastTopUpProvider = provider;
        LastTopUpAt = DateTimeOffset.UtcNow;
    }
}