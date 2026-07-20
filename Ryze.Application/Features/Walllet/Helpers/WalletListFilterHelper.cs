using Ryze.Application.Features.Walllet.Contexts.Getters;
using WalletEntity = Ryze.Domain.Features.Wallet.Entity.Wallet;

namespace Ryze.Application.Features.Walllet.Helpers;

/// <summary>
/// Provides wallet filtering rules used by wallet listing queries.
/// </summary>
/// <remarks>
/// Applies optional filters defined in <see cref="WalletListContext"/>
/// against wallet domain entities.
///
/// The helper keeps query filtering logic separate from application services
/// and supports filtering by ownership, lifecycle status, wallet type, tags,
/// and wallet creation time range.
///
/// Date comparisons are normalized to UTC to ensure consistent behavior
/// regardless of the source <see cref="DateTimeKind"/>.
/// </remarks>
public static class WalletListFilterHelper
{
    /// <summary>
    /// Determines whether a wallet matches the supplied listing criteria.
    /// </summary>
    /// <param name="wallet">
    /// Wallet aggregate to evaluate.
    /// </param>
    /// <param name="context">
    /// Query context containing optional wallet filters.
    /// </param>
    /// <returns>
    /// <c>true</c> when the wallet satisfies all active filters;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool Matches(WalletEntity wallet, WalletListContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.OwnerId) &&
            wallet.Owners.All(o => !string.Equals(o.OwnerId, context.OwnerId, StringComparison.Ordinal)))
            return false;

        if (context.Status.HasValue && wallet.Status != context.Status.Value)
            return false;

        if (context.WalletType.HasValue && wallet.Type != context.WalletType.Value)
            return false;

        if (context.Tags.Count > 0 && !ContainsAllTags(wallet, context.Tags))
            return false;

        var createdAtUtc = ToUtcDateTime(wallet.CreatedAt);
        if (context.CreatedAfter.HasValue && createdAtUtc < context.CreatedAfter.Value.UtcDateTime)
            return false;

        if (context.CreatedBefore.HasValue && createdAtUtc > context.CreatedBefore.Value.UtcDateTime)
            return false;

        return true;
    }

    /// <summary>
    /// Checks whether a wallet contains all requested tags.
    /// </summary>
    /// <remarks>
    /// Tag comparison is case-insensitive and ignores empty values.
    /// Both wallet tags and requested filters are normalized before comparison.
    /// </remarks>
    private static bool ContainsAllTags(WalletEntity wallet, IReadOnlyCollection<string> expectedTags)
    {
        var source = wallet.Tags.Values
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in expectedTags)
        {
            if (string.IsNullOrWhiteSpace(tag))
                continue;

            if (!source.Contains(tag.Trim()))
                return false;
        }

        return true;
    }

    private static DateTime ToUtcDateTime(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
