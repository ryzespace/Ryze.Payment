using Microsoft.Extensions.Options;

namespace Ryze.Infrastructure.Features.Health.Options.Validators;

/// <summary>
/// Provides base implementation for validating strongly typed options.
/// </summary>
/// <remarks>
/// Defines reusable validation pipeline for application configuration
/// objects. Derived validators provide only the specific validation rules,
/// while this class handles error collection and conversion into the standard
/// options validation result format.
///
/// This abstraction keeps configuration validation consistent across different
/// health infrastructure components.
/// </remarks>
/// <typeparam name="T">The options type being validated. </typeparam>
public abstract class OptionsValidator<T> : IValidateOptions<T>
    where T : class
{
    /// <summary>
    /// Validates the provided options instance.
    /// </summary>
    /// <remarks>
    /// Executes all validation rules defined by the derived validator and
    /// returns successful validation result when no violations are found.
    /// Otherwise, all validation errors are combined into a single failure
    /// result.
    /// </remarks>
    /// <param name="name">The name of the options instance being validated. </param>
    /// <param name="options">The options instance to validate. </param>
    /// <returns>A validation result indicating whether the options are valid.</returns>
    public ValidateOptionsResult Validate(
        string? name,
        T options)
    {
        var errors = new List<string>();

        AddRules(options, errors);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                string.Join("; ", errors));
    }

    /// <summary>
    /// Defines validation rules for specific options type.
    /// </summary>
    /// <param name="options">The options instance being validated. </param>
    /// <param name="errors">The collection where validation failures should be added. </param>
    protected abstract void AddRules(
        T options,
        List<string> errors);
}