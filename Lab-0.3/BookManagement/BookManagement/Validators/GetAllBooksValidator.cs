using BookManagement.Features.Books;
using FluentValidation;

namespace BookManagement.Validators;

public class GetAllBooksValidator: AbstractValidator<GetAllBooksRequest>
{
    public GetAllBooksValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
