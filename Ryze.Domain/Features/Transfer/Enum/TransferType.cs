namespace Ryze.Domain.Features.Transfer.Enum;

/// <summary>
/// Defines the execution type of transfer.
/// </summary>
/// <remarks>
/// Transfer type determines the business purpose and processing rules
/// applied during transfer execution.
/// </remarks>
public enum TransferType
{
    /// <summary>
    /// Internal wallet to wallet transfer.
    /// </summary>
    Internal = 0,

    /// <summary>
    /// External transfer requiring external settlement processing.
    /// </summary>
    External = 1,

    /// <summary>
    /// Transfer representing a fee charge or fee allocation.
    /// </summary>
    Fee = 2,

    /// <summary>
    /// Compensating transfer reversing a previous transaction.
    /// </summary>
    Reversal = 3
}