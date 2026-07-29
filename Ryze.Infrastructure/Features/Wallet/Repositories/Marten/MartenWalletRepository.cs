using Marten;
using Ryze.Domain.Features.Wallet.Repositories;

namespace Ryze.Infrastructure.Features.Wallet.Repositories.Marten;

/// <summary>
/// Provides a Marten-based implementation of the wallet repository.
/// </summary>
/// <param name="session">The Marten document session used to persist and query wallet documents.</param>
/// <remarks>
/// Uses Marten for document persistence and querying of wallet entities.
/// Changes are persisted immediately through the provided document session
/// when add or update operations are performed.
/// </remarks>
public sealed class MartenWalletRepository(IDocumentSession session) : IWalletRepository
{
    /// <summary>
    /// Retrieves a wallet by its unique identifier.
    /// </summary>
    /// <param name="walletId">The unique identifier of the wallet.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>
    /// A task containing the wallet if it exists; otherwise, <see langword="null" />.
    /// </returns>
    public async Task<Domain.Features.Wallet.Entity.Wallet?> GetByIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
        => await session.LoadAsync<Domain.Features.Wallet.Entity.Wallet>(
            walletId,
            cancellationToken);

    /// <summary>
    /// Retrieves all wallets stored in the document database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>
    /// A task containing a collection of all wallets currently stored in the repository.
    /// </returns>
    public async Task<IReadOnlyCollection<Domain.Features.Wallet.Entity.Wallet>> ListAsync(
        CancellationToken cancellationToken = default)
        => await session.Query<Domain.Features.Wallet.Entity.Wallet>()
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Counts the number of wallets owned by the specified owner.
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <returns>
    /// The number of wallets associated with the specified owner.
    /// </returns>
    public int CountWalletsByOwner(string ownerId)
        => session.Query<Domain.Features.Wallet.Entity.Wallet>()
            .Count(w => w.Owners.Any(o => o.OwnerId == ownerId));

    /// <summary>
    /// Adds new wallet to the document database.
    /// </summary>
    /// <param name="wallet">The wallet to add.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    public async Task AddAsync(
        Domain.Features.Wallet.Entity.Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        session.Store(wallet);
        await session.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing wallet in the document database.
    /// </summary>
    /// <remarks>
    /// The wallet is stored using its existing document identifier and the changes
    /// are persisted immediately through the current Marten session.
    /// </remarks>
    /// <param name="wallet">The wallet containing the updated state.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    public async Task UpdateAsync(
        Domain.Features.Wallet.Entity.Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        session.Store(wallet);
        await session.SaveChangesAsync(cancellationToken);
    }
}