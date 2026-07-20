using FluentValidation;
using Ryze.Application.Features.Walllet.UseCase.Commands.Requests;

namespace Ryze.Application.Features.Walllet.UseCase.Commands.Validations;

/// <summary>
/// Validates <see cref="SuspendWalletCommand"/> input before wallet suspension.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures the target wallet identifier is provided.</item>
/// <item>Ensures a non-empty suspension reason is provided.</item>
/// </list>
/// </remarks>
public class SuspendWalletCommandValidator : AbstractValidator<SuspendWalletCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="SuspendWalletCommand"/>.
    /// </summary>
    public SuspendWalletCommandValidator()
    {
        RuleFor(x => x.SuspendContext.WalletId)
            .NotEmpty()
            .WithMessage("Wallet ID must not be empty.");

        RuleFor(x => x.SuspendContext.Reason)
            .NotEmpty()
            .WithMessage("Reason must not be empty.");
    }
}
