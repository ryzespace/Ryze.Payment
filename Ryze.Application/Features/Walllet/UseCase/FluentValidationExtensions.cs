using FluentValidation;

namespace Ryze.Application.Features.Walllet.UseCase;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeGuid<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value => Guid.TryParse(value, out _))
            .WithMessage("'{PropertyName}' must be a valid UUID (GUID).");
    }
}