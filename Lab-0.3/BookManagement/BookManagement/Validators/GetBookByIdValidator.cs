using BookManagement.Features.Books;
using FluentValidation;

namespace BookManagement.Validators;

public class GetBookByIdValidator: AbstractValidator<GetBookByIdRequest>
{
    public GetBookByIdValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Book ID must not be empty.");
    }
}