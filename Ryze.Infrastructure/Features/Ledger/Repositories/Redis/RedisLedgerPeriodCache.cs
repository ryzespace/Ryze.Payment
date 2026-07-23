using Microsoft.Extensions.Logging;
using Ryze.Domain.Features.Ledger.Entity;
using StackExchange.Redis;

namespace Ryze.Infrastructure.Features.Ledger.Repositories.Redis;

/// <summary>
/// Provides process local cache for ledger accounting periods with
/// cross instance cache invalidation through Redis pub/sub.
/// </summary>
/// <remarks>
/// The cache stores the complete collection of ledger periods in memory
/// and lazily loads it through the supplied loader function when the cache
/// is empty.
///
/// Redis pub/sub is used to propagate invalidation notifications between
/// application instances. When an invalidation message is received, the
/// local cached period collection is discarded and reloaded on the next
/// access.
/// </remarks>
public sealed class RedisLedgerPeriodCache : IDisposable
{
    private const string InvalidationChannel = "ledger-periods:invalidate";

    private readonly IConnectionMultiplexer _muxer;
    private readonly ILogger<RedisLedgerPeriodCache>? _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IReadOnlyList<LedgerPeriod>? _cached;

    /// <summary>
    /// Initializes subscribes to the Redis ledger period invalidation channel.
    /// </summary>
    /// <param name="muxer">Redis connection multiplexer used for pub/sub communication. </param>
    /// <param name="logger">logger used to record cache invalidation publication failures. </param>
    public RedisLedgerPeriodCache(
        IConnectionMultiplexer muxer,
        ILogger<RedisLedgerPeriodCache>? logger = null)
    {
        _muxer = muxer;
        _logger = logger;

        _muxer.GetSubscriber().Subscribe(
            RedisChannel.Literal(InvalidationChannel),
            (_, _) => _cached = null);
    }

    /// <summary>
    /// Returns the cached ledger periods or loads them using the supplied
    /// asynchronous loader when the cache is empty.
    /// </summary>
    /// <remarks>
    /// The loader is executed only once when concurrent callers encounter
    /// an empty cache. Subsequent callers receive the populated in-memory
    /// collection until the cache is explicitly invalidated or an invalidation
    /// message is received through Redis pub/sub.
    /// </remarks>
    /// <param name="loader"> Asynchronous function used to retrieve ledger periods when the cache does not contain valid value. </param>
    /// <param name="ct">Token that can be used to cancel the cache loading operation. </param>
    /// <returns>The cached or freshly loaded collection of ledger periods. </returns>
    public async Task<IReadOnlyList<LedgerPeriod>> GetOrLoadAsync(
        Func<CancellationToken, Task<IReadOnlyList<LedgerPeriod>>> loader,
        CancellationToken ct)
    {
        if (_cached is not null)
            return _cached;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cached is not null)
                return _cached;

            _cached = await loader(ct);
            return _cached;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Invalidates the local ledger period cache and publishes an invalidation
    /// notification to other application instances through Redis pub/sub.
    /// </summary>
    /// <remarks>
    /// If Redis publication fails, the local cache is still invalidated.
    /// The failure is logged and does not propagate to the caller because
    /// cache invalidation is an optimization and should not cause the
    /// underlying ledger period persistence operation to fail.
    /// </remarks>
    public async Task InvalidateAsync()
    {
        _cached = null;

        try
        {
            await _muxer.GetSubscriber()
                .PublishAsync(
                    RedisChannel.Literal(InvalidationChannel),
                    RedisValue.EmptyString)
                .ConfigureAwait(false);
        }
        catch (RedisException ex)
        {
            _logger?.LogWarning(
                ex,
                "Failed to publish ledger period cache invalidation.");
        }
    }

    /// <summary>
    /// Releases resources associated with the Redis subscription.
    /// </summary>
    public void Dispose() =>
        _muxer.GetSubscriber()
            .Unsubscribe(RedisChannel.Literal(InvalidationChannel));
}