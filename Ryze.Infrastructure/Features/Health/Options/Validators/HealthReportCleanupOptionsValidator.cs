namespace Ryze.Infrastructure.Features.Health.Options.Validators;

/// <summary>
/// Validates configuration options used for health report cleanup and retention.
/// </summary>
/// <remarks>
/// Ensures that cleanup scheduling and retention values are valid before
/// historical health report maintenance begins. Invalid values could prevent
/// cleanup execution or cause unintended deletion behavior.
/// </remarks>
public sealed class HealthReportCleanupOptionsValidator 
    : OptionsValidator<HealthReportCleanupOptions>
{
    /// <summary>
    /// Adds validation rules for <see cref="HealthReportCleanupOptions"/>.
    /// </summary>
    /// <param name="options">The health report cleanup configuration instance being validated. </param>
    /// <param name="errors">The collection where validation failures are appended. </param>
    protected override void AddRules(
        HealthReportCleanupOptions options,
        List<string> errors)
    {
        if (options.Interval <= TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthReportCleanupOptions.Interval)} must be greater than zero.");
        }

        if (options.Retention <= TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthReportCleanupOptions.Retention)} must be greater than zero.");
        }
    }
}