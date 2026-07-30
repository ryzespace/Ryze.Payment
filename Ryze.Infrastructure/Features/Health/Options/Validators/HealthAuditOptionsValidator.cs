namespace Ryze.Infrastructure.Features.Health.Options.Validators;

/// <summary>
/// Validates configuration options used by periodic health audit execution.
/// </summary>
/// <remarks>
/// Ensures that audit scheduling values are valid before background health audit
/// processing begins. Prevents invalid timing configuration that could disable
/// or destabilize scheduled health evaluation.
/// </remarks>
public sealed class HealthAuditOptionsValidator : OptionsValidator<HealthAuditOptions>
{
    /// <summary>
    /// Adds validation rules for <see cref="HealthAuditOptions"/>.
    /// </summary>
    /// <param name="options">The health audit configuration instance being validated. </param>
    /// <param name="errors">The collection where validation failures are appended. </param>
    protected override void AddRules(
        HealthAuditOptions options,
        List<string> errors)
    {
        if (options.Interval <= TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthAuditOptions.Interval)} must be greater than zero.");
        }

        if (options.StartupJitter < TimeSpan.Zero)
        {
            errors.Add($"{nameof(HealthAuditOptions.StartupJitter)} must not be negative.");
        }
    }
}