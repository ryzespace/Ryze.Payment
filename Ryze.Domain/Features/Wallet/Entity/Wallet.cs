using Ryze.Domain.Features.Shared;
using Ryze.Domain.Features.Shared.Enum;
using Ryze.Domain.Features.Wallet.ValueObject;
using Ryze.Domain.Features.Wallet.ValueObject.WalletOnwers.WalletVo;

namespace Ryze.Domain.Features.Wallet.Entity;

/// <summary>
/// Wallet aggregate root representing account lifecycle, ownership, and balance metadata.
/// </summary>
/// <remarks>
/// Encapsulates lifecycle transitions (<c>Active</c>, <c>Suspended</c>, <c>Closed</c>)
/// and guards domain rules for wallet state changes.
/// </remarks>
public sealed class Wallet
{
    /// <summary>
    /// Unique wallet identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Wallet classification (for example, personal/business).
    /// </summary>
    public WalletType Type { get; private set; }

    /// <summary>
    /// Wallet base currency.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public WalletStatus Status { get; private set; }

    private readonly List<WalletOwner> _owners = [];

    /// <summary>
    /// Wallet owners associated with this aggregate.
    /// </summary>
    public IReadOnlyCollection<WalletOwner> Owners => _owners;

    /// <summary>
    /// Current wallet balance snapshot.
    /// </summary>
    public WalletBalance.Entity.WalletBalance Balance { get; set; } = null!;

    /// <summary>
    /// Arbitrary wallet metadata key-value collection.
    /// </summary>
    public Metadata Metadata { get; private set; }

    /// <summary>
    /// Wallet tags used for classification and filtering.
    /// </summary>
    public Tags Tags { get; private set; }

    /// <summary>
    /// Optional account number assigned to this wallet.
    /// </summary>
    public AccountNumber? AccountNumber { get; private set; }

    /// <summary>
    /// Optional routing number assigned to this wallet.
    /// </summary>
    public RoutingNumber? RoutingNumber { get; private set; }

    /// <summary>
    /// Wallet creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Last update timestamp in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Wallet closure timestamp in UTC.
    /// </summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>
    /// Initializes a new wallet aggregate.
    /// </summary>
    /// <param name="id">Wallet identifier.</param>
    /// <param name="type">Wallet type.</param>
    /// <param name="currency">Wallet currency.</param>
    /// <param name="owners">Initial wallet owner collection.</param>
    /// <param name="accountNumber">Optional account number.</param>
    /// <param name="routingNumber">Optional routing number.</param>
    /// <param name="metadata">Optional metadata payload.</param>
    /// <param name="tags">Optional tag payload.</param>
    /// <exception cref="DomainException">Thrown when an owner collection is empty.</exception>
    public Wallet(
        Guid id,
        WalletType type,
        Currency currency,
        IEnumerable<WalletOwner> owners,
        AccountNumber? accountNumber = null,
        RoutingNumber? routingNumber = null,
        Metadata? metadata = null,
        Tags? tags = null)
    {
        if (owners is null || !owners.Any())
            throw new DomainException("Wallet must have at least one owner.");

        Id = id;
        Type = type;
        Currency = currency;
        Status = WalletStatus.Active;

        _owners.AddRange(owners);

        AccountNumber = accountNumber;
        RoutingNumber = routingNumber;

        Metadata = metadata ?? Metadata.Empty;
        Tags = tags ?? Tags.Empty;

        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspends an active wallet for business reason.
    /// </summary>
    /// <param name="reason">Reason used for audit metadata.</param>
    /// <exception cref="DomainException">Thrown when the wallet is not in an active state.</exception>
    public void Suspend(string reason)
    {
        if (Status != WalletStatus.Active)
            throw new DomainException("Only active wallets can be suspended.");

        Status = WalletStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        Metadata = Metadata.With("suspendReason", reason);
    }

    /// <summary>
    /// Reactivates a suspended wallet with a business reason.
    /// </summary>
    /// <param name="reason">Reason used for audit metadata.</param>
    /// <exception cref="DomainException">Thrown when wallet is not in suspended state.</exception>
    public void Reactivate(string reason)
    {
        if (Status != WalletStatus.Suspended)
            throw new DomainException("Only suspended wallets can be reactivated.");

        Status = WalletStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        Metadata = Metadata.With("reactivateReason", reason);
    }

    /// <summary>
    /// Closes wallet permanently with a business reason.
    /// </summary>
    /// <param name="reason">Reason used for audit metadata.</param>
    /// <exception cref="DomainException">Thrown when wallet is already closed.</exception>
    public void Close(string reason)
    {
        if (Status == WalletStatus.Closed)
            throw new DomainException("Wallet is already closed.");

        Status = WalletStatus.Closed;
        ClosedAt = DateTime.UtcNow;

        Metadata = Metadata.With("closeReason", reason);
    }
}
