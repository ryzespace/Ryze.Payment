using FluentValidation;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Validations;

/// <summary>
/// Validates <see cref="ReactivateWalletCommand"/> input before wallet reactivation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures the target wallet identifier is provided.</item>
/// <item>Ensures a non-empty reactivation reason is provided.</item>
/// </list>
/// </remarks>
public class ReactivateWalletCommandValidator : AbstractValidator<ReactivateWalletCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="ReactivateWalletCommand"/>.
    /// </summary>
    public ReactivateWalletCommandValidator()
    {
        RuleFor(x => x.ReactivateContext.WalletId)
            .NotEmpty()
            .WithMessage("Wallet ID must not be empty.");

        RuleFor(x => x.ReactivateContext.Reason)
            .NotEmpty()
            .WithMessage("Reason must not be empty.");
    }
}
