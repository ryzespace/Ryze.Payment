namespace Ryze.Domain.Features.Ledger.Exceptions;

/// <summary>
/// Represents failure during ledger domain validation.
/// </summary>
/// <remarks>
/// This exception is thrown when an entity, operation, or transaction violates
/// one or more ledger business rules.
///
/// Validation failures indicate that the requested operation cannot produce a
/// valid accounting state and must be corrected before being persisted.
/// </remarks>
public sealed class LedgerValidationException(
    string message,
    IEnumerable<string>? errors = null)
    : LedgerException(message, "LEDGER_VALIDATION_FAILED")
{
    /// <summary>
    /// Gets the collection of detailed validation errors.
    /// </summary>
    /// <remarks>
    /// Contains individual rule violations when multiple validation problems
    /// are detected during single operation.
    /// </remarks>
    public IReadOnlyList<string> Errors { get; } = errors?.ToList() ?? [];
}