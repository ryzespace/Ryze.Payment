using FluentValidation;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Validations;

/// <summary>
/// Validates <see cref="CreateWalletCommand"/> input before wallet creation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures at least one wallet owner is provided.</item>
/// <item>Ensures each owner has a non-empty and valid GUID <c>OwnerId</c>.</item>
/// </list>
/// </remarks>
public class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="CreateWalletCommand"/>.
    /// </summary>
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.Context.Owners)
            .NotEmpty()
            .WithMessage("At least one wallet owner is required.");

        RuleForEach(x => x.Context.Owners)
            .ChildRules(owner =>
            {
                owner.RuleFor(x => x.OwnerId)
                    .NotEmpty().WithMessage("Owner ID must not be empty.")
                    .MustBeGuid();
            });
    }
}
