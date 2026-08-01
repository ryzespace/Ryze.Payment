namespace Ryze.Domain.Features.Transfer.Enum;

/// <summary>
/// Represents the lifecycle status of transfer intent.
/// </summary>
/// <remarks>
/// Transfer intent tracks transfer request from initial data collection,
/// through confirmation and execution, until an outcome is reached.
/// </remarks>
public enum TransferIntentStatus
{
    /// <summary>
    /// The source wallet has not been provided yet.
    /// </summary>
    RequiresSource = 0,

    /// <summary>
    /// The destination wallet has not been provided yet.
    /// </summary>
    RequiresDestination = 1,

    /// <summary>
    /// Required transfer information is available and user confirmation is required.
    /// </summary>
    RequiresConfirmation = 2,

    /// <summary>
    /// The transfer intent has been confirmed and is being processed.
    /// </summary>
    Processing = 3,

    /// <summary>
    /// The transfer was completed successfully.
    /// </summary>
    Succeeded = 4,

    /// <summary>
    /// The transfer execution failed.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// The transfer intent was canceled.
    /// </summary>
    Cancelled = 6
}