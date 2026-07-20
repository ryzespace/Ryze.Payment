namespace Ryze.Domain.Features.Ledger.Exceptions;

/// <summary>
/// Base exception type for all ledger domain errors.
/// </summary>
/// <remarks>
/// Provides  common abstraction for exceptions raised inside the ledger domain.
/// Derived exceptions should represent specific business rule violations,
/// validation failures, or consistency problems within accounting operations.
///
/// The exception includes a stable error code that can be used by application
/// layers for logging, monitoring, API error mapping, and diagnostics.
/// </remarks>
public abstract class LedgerException(string message, string code = "LEDGER_ERROR") : Exception(message)
{
    /// <summary>
    /// Gets the error code associated with this ledger exception.
    /// </summary>
    public string Code { get; } = code;
}