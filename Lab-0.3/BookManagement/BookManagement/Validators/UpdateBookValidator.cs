using BookManagement.Features.Books;
using FluentValidation;

namespace BookManagement.Validators;

public class UpdateBookValidator: AbstractValidator<UpdateBookRequest>
{
    public UpdateBookValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Book ID must not be empty.");
        RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Year).InclusiveBetween(0, DateTime.Now.Year).WithMessage($"Year must be between 0 and {DateTime.Now.Year}.");
    }
}