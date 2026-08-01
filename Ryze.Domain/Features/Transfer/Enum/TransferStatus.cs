namespace Ryze.Domain.Features.Transfer.Enum;

/// <summary>
/// Represents the lifecycle state of transfer.
/// </summary>
/// <remarks>
/// Transfer status describes the current execution state of funds movement.
/// State transitions should be performed through domain operations and events
/// to preserve consistency across ledger, wallet, and payment flows.
/// </remarks>
public enum TransferStatus
{
    /// <summary>
    /// Transfer has been created but has not reached a final state.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transfer has been successfully completed and funds movement finished.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Transfer execution failed and funds movement was not completed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Transfer was compensated by reversal operation.
    /// </summary>
    Reversed = 3
}