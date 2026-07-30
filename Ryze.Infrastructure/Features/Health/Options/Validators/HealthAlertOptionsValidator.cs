namespace Ryze.Infrastructure.Features.Health.Options.Validators;

/// <summary>
/// Validates configuration options used by health alert monitoring.
/// </summary>
/// <remarks>
/// Ensures that alert evaluation settings contain valid values before the
/// application starts using them. Invalid configuration prevents predictable
/// alert processing behavior and is rejected during options validation.
/// </remarks>
public sealed class HealthAlertOptionsValidator : OptionsValidator<HealthAlertOptions>
{
    /// <summary>
    /// Adds validation rules for <see cref="HealthAlertOptions"/>.
    /// </summary>
    /// <param name="options">The health alert configuration instance being validated. </param>
    /// <param name="errors">The collection where validation failures are appended.</param>
    protected override void AddRules(
        HealthAlertOptions options,
        List<string> errors)
    {
        if (options.ConsecutiveFailureThreshold <= 0)
        {
            errors.Add($"{nameof(HealthAlertOptions.ConsecutiveFailureThreshold)} must be greater than zero.");
        }

        if (options.Interval <= TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthAlertOptions.Interval)} must be greater than zero.");
        }
    }
}