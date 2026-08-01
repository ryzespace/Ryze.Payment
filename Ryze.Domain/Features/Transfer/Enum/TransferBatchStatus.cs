namespace Ryze.Domain.Features.Transfer.Enum;

/// <summary>
/// Represents the execution state of transfer batch.
/// </summary>
/// <remarks>
/// Transfer batch groups multiple transfers into a single logical operation.
/// The batch progresses from <see cref="Pending"/> to
/// <see cref="Processing"/>, and then completes in one of the terminal
/// states: <see cref="Completed"/>, <see cref="Partial"/>, or
/// <see cref="Failed"/>.
/// </remarks>
public enum TransferBatchStatus
{
    /// <summary>
    /// The batch has been created but processing has not started.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The batch is currently being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// All transfers in the batch completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The batch completed with mix of successful and failed transfers.
    /// </summary>
    Partial = 3,

    /// <summary>
    /// The batch failed before completing successfully.
    /// </summary>
    Failed = 4
}