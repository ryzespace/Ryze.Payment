using Ryze.Application.Features.Ledger.Interfaces;
using Ryze.Application.Features.Ledger.Verification;

namespace Ryze.Infrastructure.Features.Ledger.Services;

/// <summary>
/// Thread safe in-memory implementation of <see cref="ILedgerIntegrityTracker"/>
/// that stores the most recently generated ledger integrity report.
/// </summary>
/// <remarks>
/// The latest report is maintained in process memory and protected by synchronization
/// lock to ensure consistent access when reports are generated or retrieved concurrently.
/// The stored report is lost when the application process restarts.
/// </remarks>
public sealed class LedgerIntegrityTracker : ILedgerIntegrityTracker
{
    private IntegrityReport? _latestReport;
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets the most recently recorded ledger integrity report.
    /// </summary>
    /// <returns>
    /// The latest <see cref="IntegrityReport"/>, or <c>null</c> if no integrity
    /// report has been generated and stored yet.
    /// </returns>
    public IntegrityReport? GetLatestReport()
    {
        lock (_lock)
        {
            return _latestReport;
        }
    }

    /// <summary>
    /// Updates the tracker with newly generated ledger integrity report.
    /// </summary>
    /// <param name="report">The latest ledger integrity report to store in memory.</param>
    public void UpdateReport(IntegrityReport report)
    {
        lock (_lock)
        {
            _latestReport = report;
        }
    }
}