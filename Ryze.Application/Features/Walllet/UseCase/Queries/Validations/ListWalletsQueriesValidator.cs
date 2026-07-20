using FluentValidation;
using Ryze.Application.Features.Walllet.UseCase.Queries.Requests;

namespace Ryze.Application.Features.Walllet.UseCase.Queries.Validations;

/// <summary>
/// Validates <see cref="ListWalletsQueries"/> input before wallet listing.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures pagination parameters are valid.</item>
/// <item>Ensures optional owner filter has a valid GUID format.</item>
/// <item>Ensures tag filters do not contain empty values.</item>
/// <item>Ensures created-at range is chronologically valid when both bounds are provided.</item>
/// </list>
/// </remarks>
public class ListWalletsQueriesValidator : AbstractValidator<ListWalletsQueries>
{
    /// <summary>
    /// Initializes validation rules for <see cref="ListWalletsQueries"/>.
    /// </summary>
    public ListWalletsQueriesValidator()
    {
        RuleFor(x => x.Context.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than zero.");

        RuleFor(x => x.Context.PageToken)
            .Must(pageToken => pageToken is null || !string.IsNullOrWhiteSpace(pageToken))
            .WithMessage("Page token must not be empty.");

        RuleFor(x => x.Context.OwnerId)
            .Must(ownerId => string.IsNullOrWhiteSpace(ownerId) || Guid.TryParse(ownerId, out _))
            .WithMessage("Owner ID must be a valid UUID (GUID).");

        RuleForEach(x => x.Context.Tags)
            .Must(tag => !string.IsNullOrWhiteSpace(tag))
            .WithMessage("Tags must not contain empty values.");

        RuleFor(x => x.Context)
            .Must(context => !context.CreatedAfter.HasValue ||
                             !context.CreatedBefore.HasValue ||
                             context.CreatedAfter.Value < context.CreatedBefore.Value)
            .WithMessage("CreatedAfter must be earlier than CreatedBefore.");
    }
}
