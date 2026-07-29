using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Ryze.Domain.Features.Wallet.Repositories;

namespace Ryze.Infrastructure.Features.Wallet.Repositories.InMemory;

/// <summary>
/// Provides an in-memory implementation of the wallet repository.
/// </summary>
/// <param name="logger">The logger used to record wallet repository operations.</param>
/// <remarks>
/// Stores wallet entities in process local concurrent dictionary.
/// This implementation is intended primarily for development, testing,
/// and other scenarios where persistent storage is not required.
/// </remarks>
public sealed class InMemoryWalletRepository(
    ILogger<InMemoryWalletRepository> logger) : IWalletRepository
{
    private readonly ConcurrentDictionary<Guid, Domain.Features.Wallet.Entity.Wallet> _store = new();

    private readonly Lock _sync = new();

    /// <summary>
    /// Retrieves wallet by its unique identifier.
    /// </summary>
    /// <param name="walletId">The unique identifier of the wallet.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>
    /// A task containing the wallet if it exists; otherwise, <see langword="null" />.
    /// </returns>
    public Task<Domain.Features.Wallet.Entity.Wallet?> GetByIdAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(walletId, out var wallet);

        logger.LogDebug(
            "GetByIdAsync called for WalletId={WalletId}, found={Found}",
            walletId,
            wallet != null);

        return Task.FromResult(wallet);
    }

    /// <summary>
    /// Retrieves all wallets currently stored in memory.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>
    /// A task containing snapshot of all wallets currently stored in the repository.
    /// </returns>
    public Task<IReadOnlyCollection<Domain.Features.Wallet.Entity.Wallet>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var result = _store.Values.ToList();

        return Task.FromResult<IReadOnlyCollection<Domain.Features.Wallet.Entity.Wallet>>(result);
    }

    /// <summary>
    /// Counts the number of wallets owned by the specified owner.
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>
    /// A task containing the number of wallets associated with the specified owner.
    /// </returns>
    public Task<int> CountWalletsByOwnerAsync(
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var count = _store.Values.Count(
                w => w.Owners.Any(o => o.OwnerId == ownerId));

            return Task.FromResult(count);
        }
    }

    /// <summary>
    /// Counts the number of wallets owned by the specified owner.
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <returns>
    /// The number of wallets associated with the specified owner.
    /// </returns>
    public int CountWalletsByOwner(string ownerId) =>
        _store.Values.Count(w => w.Owners.Any(o => o.OwnerId == ownerId));

    /// <summary>
    /// Adds new wallet to the in-memory repository.
    /// </summary>
    /// <param name="wallet">The wallet to add.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a wallet with the same identifier already exists.
    /// </exception>
    public Task AddAsync(
        Domain.Features.Wallet.Entity.Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryAdd(wallet.Id, wallet))
        {
            logger.LogWarning(
                "Attempted to add existing WalletId={WalletId}",
                wallet.Id);

            throw new InvalidOperationException(
                $"Wallet with Id {wallet.Id} already exists.");
        }

        logger.LogInformation(
            "Wallet added: WalletId={WalletId}",
            wallet.Id);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates an existing wallet in the in-memory repository.
    /// </summary>
    /// <remarks>
    /// If wallet with the specified identifier does not exist, the wallet is added
    /// to the repository.
    /// </remarks>
    /// <param name="wallet">The wallet containing the updated state.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    public Task UpdateAsync(
        Domain.Features.Wallet.Entity.Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        _store[wallet.Id] = wallet;

        logger.LogInformation(
            "Wallet updated: WalletId={WalletId}, Status={Status}",
            wallet.Id,
            wallet.Status);

        return Task.CompletedTask;
    }
}