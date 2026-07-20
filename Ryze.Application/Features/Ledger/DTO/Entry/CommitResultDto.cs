namespace Ryze.Application.Features.Ledger.DTO.Entry;

/// <summary>
/// Represents the result of committing pending ledger entries.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of commit operation
/// performed on pending ledger entries.
///
/// Contains the number of entries successfully committed, the aggregated
/// committed amount, and the currency context of the operation.
/// </remarks>
/// <param name="EntriesCommitted"> Number of ledger entries successfully committed. </param>
/// <param name="TotalAmount">Total monetary amount represented by the committed entries.</param>
/// <param name="Currency">Currency associated with the committed entries.</param>
public sealed record CommitResultDto(
    int EntriesCommitted,
    decimal TotalAmount,
    string Currency
);
