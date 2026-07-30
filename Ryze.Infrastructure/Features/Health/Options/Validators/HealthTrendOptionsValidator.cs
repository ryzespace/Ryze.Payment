namespace Ryze.Infrastructure.Features.Health.Options.Validators;

/// <summary>
/// Validates configuration options used for health trend analysis.
/// </summary>
/// <remarks>
/// Ensures that trend processing configuration contains valid scheduling,
/// processing limit, and availability threshold values before background
/// health trend calculations are executed.
/// </remarks>
public sealed class HealthTrendOptionsValidator 
    : OptionsValidator<HealthTrendOptions>
{
    /// <summary>
    /// Adds validation rules for <see cref="HealthTrendOptions"/>.
    /// </summary>
    /// <param name="options">The health trend configuration instance being validated.</param>
    /// <param name="errors">The collection where validation failures are appended.</param>
    protected override void AddRules(
        HealthTrendOptions options,
        List<string> errors)
    {
        if (options.Interval <= TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthTrendOptions.Interval)} must be greater than zero.");
        }

        if (options.StartupJitter < TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthTrendOptions.StartupJitter)} must not be negative.");
        }

        if (options.MaxReportsPerWindow <= 0)
        {
            errors.Add($"{nameof(HealthTrendOptions.MaxReportsPerWindow)} must be greater than zero.");
        }

        if (options.AvailabilityThresholdPercent is < 0 or > 100)
        {
            errors.Add($"{nameof(HealthTrendOptions.AvailabilityThresholdPercent)} must be between 0 and 100.");
        }
    }
}