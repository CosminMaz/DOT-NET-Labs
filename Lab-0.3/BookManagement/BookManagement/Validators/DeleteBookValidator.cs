using BookManagement.Features.Books;
using FluentValidation;

namespace BookManagement.Validators;

public class DeleteBookValidator: AbstractValidator<DeleteBookRequest>
{
    public DeleteBookValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Book ID must not be empty.");
    }
}