using WalletEntity = Ryze.Domain.Features.Wallet.Entity.Wallet;

namespace Ryze.Domain.Features.Wallet.Repositories;

/// <summary>
/// Repository abstraction for wallet aggregate persistence and retrieval.
/// </summary>
/// <remarks>
/// Defines the domain facing persistence boundary for wallet aggregates.
/// Implementations are responsible for loading and storing wallet state
/// while hiding underlying storage details from the domain and application
/// layers.
///
/// The repository operates on complete wallet aggregates and should preserve
/// aggregate consistency when applying state changes.
/// </remarks>
public interface IWalletRepository
{
    /// <summary>
    /// Persists a newly created wallet aggregate.
    /// </summary>
    /// <param name="wallet">
    /// Wallet aggregate to store.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task AddAsync(WalletEntity wallet, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Persist changes made to an existing wallet aggregate.
    /// </summary>
    /// <param name="wallet">
    /// Updated wallet aggregate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task UpdateAsync(WalletEntity wallet, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a wallet aggregate by its identifier.
    /// </summary>
    /// <param name="walletId">
    /// Identifier of the wallet aggregate.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The wallet aggregate if found; otherwise <c>null</c>.
    /// </returns>
    Task<WalletEntity?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves wallet aggregates available for listing operations.
    /// </summary>
    /// <remarks>
    /// Filtering, projection, and pagination are applied at the application
    /// query layer rather than by this repository abstraction.
    /// </remarks>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection of wallet aggregates.
    /// </returns>
    Task<IReadOnlyCollection<WalletEntity>> ListAsync(CancellationToken cancellationToken = default);


    //  Task<WalletBalanceEntity?> GetCachedBalanceAsync(Guid walletId, CancellationToken cancellationToken = default);
    //  Task<int> CountWalletsByOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
  
    /// <summary>
    /// Counts wallets associated with a specific owner.
    /// </summary>
    /// <param name="ownerId">
    /// Identifier of the wallet owner.
    /// </param>
    /// <returns>
    /// Number of wallets owned by the specified owner.
    /// </returns>
    int CountWalletsByOwner(string ownerId);
}
