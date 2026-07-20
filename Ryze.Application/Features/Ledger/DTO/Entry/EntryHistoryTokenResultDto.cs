namespace Ryze.Application.Features.Ledger.DTO.Entry;

/// <summary>
/// Represents the result of token based ledger entry history query.
/// </summary>
/// <remarks>
/// Provides read only data transfer representation of a paginated
/// ledger entry history response using continuation token pagination.
///
/// Contains the returned ledger entries together with the continuation
/// metadata required for retrieving subsequent result pages.
/// </remarks>
/// <param name="Items">Collection of ledger entries matching the requested query criteria.</param>
/// <param name="NextToken">Continuation token used to retrieve the next page of results.</param>
/// <param name="HasMore">Indicates whether additional ledger entries are available.</param>
public sealed record EntryHistoryTokenResultDto(
    IReadOnlyList<EntryItemDto> Items,
    string? NextToken,
    bool HasMore
);
