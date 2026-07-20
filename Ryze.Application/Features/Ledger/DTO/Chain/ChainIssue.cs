namespace Ryze.Application.Features.Ledger.DTO.Chain;

/// <summary>
/// Represents an issue detected during ledger chain verification.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of a ledger integrity
/// problem identified during chain validation.
///
/// Contains the affected account, issue classification, human-readable
/// description, and optional reference to the related ledger entry.
/// </remarks>
/// <param name="AccountId">Ledger account identifier affected by the issue.</param>
/// <param name="IssueType">Category or classification of the detected issue.</param>
/// <param name="Description">Human-readable description of the integrity issue.</param>
/// <param name="EntryId">
/// Optional identifier of the ledger entry associated with the issue.
/// </param>
public sealed record ChainIssue(
    string AccountId,
    string IssueType,
    string Description,
    Guid? EntryId = null
);