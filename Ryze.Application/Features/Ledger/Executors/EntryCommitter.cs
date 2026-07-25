using Microsoft.Extensions.Logging;
using Ryze.Application.Features.Ledger.DTO.Entry;
using Ryze.Application.Features.Ledger.Interfaces.Commands;
using Ryze.Application.Shared.Locking;
using Ryze.Domain.Features.Ledger.Enum;
using Ryze.Domain.Features.Ledger.Events.Account;
using Ryze.Domain.Features.Ledger.Events.Entry;
using Ryze.Domain.Features.Ledger.Repositories;
using Ryze.Domain.Shared;
using Ryze.Domain.Shared.Enum;

namespace Ryze.Application.Features.Ledger.Executors;

/// <summary>
/// Commits pending ledger entries for individual accounts or specific entry identifiers.
/// </summary>
/// <remarks>
/// Coordinates pending entry commitment by acquiring locks, resolving eligible
/// journal entries, publishing the corresponding domain events, and returning
/// the resulting commitment totals. The executor does not directly mutate
/// journal entries; state changes are propagated through domain events.
/// </remarks>
public sealed class EntryCommitter(
    IJournalEntryRepository entryRepo,
    IEventPublisher eventPublisher,
    ILockProvider lockProvider,
    ILogger<EntryCommitter> logger) : IEntryCommitter
{
    /// <summary>
    /// Commits all pending ledger entries belonging to the specified account.
    /// </summary>
    /// <param name="accountId">The identifier of the account whose pending entries should be committed.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<CommitResultDto> CommitAllPendingAsync(
        string accountId,
        CancellationToken ct = default)
    {
        await using var handle = await lockProvider.AcquireAsync(
            $"ledger:commit:account:{accountId}",
            TimeSpan.FromSeconds(30),
            ct);

        var entries = await entryRepo.GetEntriesAsync(accountId, ct);
        var pending = entries
            .Where(e => e.Posting.Status == EntryStatus.Pending)
            .ToList();

        if (pending.Count == 0)
        {
            return new CommitResultDto(0, 0,
                (pending.FirstOrDefault()?.Posting.Currency
                 ?? Currency.USD).ToString());
        }

        var currency = pending[0].Posting.Currency;
        var totalAmount = pending.Sum(e => e.Posting.Amount);
        var entryIds = pending.Select(e => e.Id).ToList();

        var @event = new AccountEntriesCleared(
            accountId,
            entryIds,
            DateTimeOffset.UtcNow);

        await eventPublisher.PublishAsync(Guid.NewGuid(), @event, ct);

        logger.LogInformation(
            "Committed {Count} pending entries for account {Account}: {Total} {Currency}",
            pending.Count,
            accountId,
            totalAmount,
            currency);

        return new CommitResultDto(
            pending.Count,
            totalAmount,
            currency.ToString());
    }

    /// <summary>
    /// Commits the specified pending ledger entries.
    /// </summary>
    /// <param name="entryIds">The identifiers of the entries to commit.</param>
    /// <param name="ct">Token that can be used to cancel the asynchronous operation.</param>
    public async Task<CommitResultDto> CommitEntriesAsync(
        IReadOnlyList<Guid> entryIds,
        CancellationToken ct = default)
    {
        if (entryIds.Count == 0)
            return new CommitResultDto(0, 0, "USD");

        var lockKey = $"ledger:commit:entries:{string.Join(",", entryIds.OrderBy(id => id))}";

        await using var handle = await lockProvider.AcquireAsync(
            lockKey,
            TimeSpan.FromSeconds(30),
            ct);

        var entries = new List<Domain.Features.Ledger.Entity.JournalEntry>();

        foreach (var id in entryIds)
        {
            var entry = await entryRepo.GetEntryByIdAsync(id, ct);

            if (entry is { Posting.Status: EntryStatus.Pending })
                entries.Add(entry);
        }

        if (entries.Count == 0)
            return new CommitResultDto(0, 0, "USD");

        var currency = entries[0].Posting.Currency;
        var totalAmount = entries.Sum(e => e.Posting.Amount);
        var clearedIds = entries.Select(e => e.Id).ToList();

        var @event = new EntriesCleared(
            clearedIds,
            DateTimeOffset.UtcNow);

        await eventPublisher.PublishAsync(Guid.NewGuid(), @event, ct);

        logger.LogInformation(
            "Committed {Count} specific entries: {Total} {Currency}",
            entries.Count,
            totalAmount,
            currency);

        return new CommitResultDto(
            entries.Count,
            totalAmount,
            currency.ToString());
    }
}