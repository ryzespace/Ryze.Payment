using FluentValidation;
using Ryze.Application.Features.Walllet.UseCase.Queries.Requests;

namespace Ryze.Application.Features.Walllet.UseCase.Queries.Validations;

/// <summary>
/// Validates <see cref="GetWalletQueries"/> input before wallet retrieval.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures the target wallet identifier is provided.</item>
/// </list>
/// </remarks>
public class GetWalletQueriesValidator : AbstractValidator<GetWalletQueries>
{
    /// <summary>
    /// Initializes validation rules for <see cref="GetWalletQueries"/>.
    /// </summary>
    public GetWalletQueriesValidator()
    {
        RuleFor(x => x.Context.WalletId)
            .NotEmpty()
            .WithMessage("Wallet ID must not be empty.");
    }
}
