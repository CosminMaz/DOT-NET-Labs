using BookManagement.Features.Books;
using FluentValidation;

namespace BookManagement.Validators;

public class GetBookByIdByPageValidator: AbstractValidator<GetBookByIdByPageRequest>
{
    public GetBookByIdByPageValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Book ID must not be empty.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
