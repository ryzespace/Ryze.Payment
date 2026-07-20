namespace Ryze.Application.Features.Ledger.DTO.Chain;

/// <summary>
/// Represents the result of ledger chain integrity verification.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of a ledger account
/// verification report.
///
/// Contains information about entry validation, calculated balances,
/// projection consistency, and optional details about detected chain
/// mismatches or integrity issues.
/// </remarks>
public sealed record BalanceChainReportDto(
    string AccountId,
    string Currency,
    int TotalEntries,
    int MatchedEntries,
    int MismatchedEntries,
    bool IsChainValid,
    decimal ComputedBalance,
    decimal ProjectedBalance,
    bool BalanceMatchesProjection,
    DateTimeOffset CheckedAt,
    IReadOnlyList<ChainEntryItem>? Entries = null,
    IReadOnlyList<ChainIssue>? Issues = null
);