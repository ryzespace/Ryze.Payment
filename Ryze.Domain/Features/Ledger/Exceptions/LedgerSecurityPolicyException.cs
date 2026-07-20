namespace Ryze.Domain.Features.Ledger.Exceptions;

/// <summary>
/// Represents violation of ledger security policy or restricted accounting rule.
/// </summary>
/// <remarks>
/// This exception is thrown when an operation attempts to perform an action
/// that violates security constraints defined for ledger accounts or financial
/// operations.
///
/// Security policies may restrict access to sensitive accounts, prevent
/// unauthorized postings, or enforce additional controls around privileged
/// ledger actions.
/// </remarks>
public sealed class LedgerSecurityPolicyException(
    string policyName,
    string accountId,
    string message)
    : LedgerException(message, "LEDGER_SECURITY_VIOLATION")
{
    /// <summary>
    /// Gets the name of the security policy that was violated.
    /// </summary>
    /// <remarks>
    /// Identifies the rule responsible for rejecting the operation,
    /// allowing monitoring and audit systems to categorize violations.
    /// </remarks>
    public string PolicyName { get; } = policyName;

    /// <summary>
    /// Gets the identifier of the account affected by the security restriction.
    /// </summary>
    /// <remarks>
    /// Used for audit logging and investigation of unauthorized or restricted
    /// ledger operations.
    /// </remarks>
    public string RestrictedAccountId { get; } = accountId;
}