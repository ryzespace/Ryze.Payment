namespace Ryze.Domain.Features.Transfer.Enum;

/// <summary>
/// Represents the lifecycle state of scheduled transfer.
/// </summary>
/// <remarks>
/// Scheduled transfer begins in <see cref="Pending"/>, transitions to
/// <see cref="Processing"/> when execution starts, and finally reaches one
/// of the terminal states <see cref="Completed"/>,
/// <see cref="Failed"/>, or <see cref="Cancelled"/>.
/// </remarks>
public enum ScheduleStatus
{
    /// <summary>
    /// The transfer is scheduled and waiting for its execution time.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// The transfer is currently being executed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// The scheduled transfer completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Execution failed after one or more attempts.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The scheduled transfer was canceled before completion.
    /// </summary>
    Cancelled = 4
}