using Ryze.Application.Features.Ledger.Verification;

namespace Ryze.Application.Features.Ledger.Interfaces;

/// <summary>
/// Tracks the latest ledger integrity verification report.
/// </summary>
/// <remarks>
/// Provides access to the most recently generated ledger integrity report
/// and allows the current report to be replaced with newly generated result.
/// </remarks>
public interface ILedgerIntegrityTracker
{
    /// <summary>
    /// Gets the most recently recorded ledger integrity report.
    /// </summary>
    /// <returns>
    /// The latest <see cref="IntegrityReport"/>, or null when no integrity
    /// report has been generated and recorded yet.
    /// </returns>
    IntegrityReport? GetLatestReport();

    /// <summary>
    /// Updates the tracker with the specified ledger integrity report.
    /// </summary>
    /// <param name="report">The latest ledger integrity report to store.</param>
    void UpdateReport(IntegrityReport report);
}