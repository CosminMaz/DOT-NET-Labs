using FluentValidation;

namespace OrderManagementAPI.Features.Order
{
    public class CreateOrderProfileRequestValidator : AbstractValidator<CreateOrderProfileRequest>
    {
        public CreateOrderProfileRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
            RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required.");
            RuleFor(x => x.ISBN).NotEmpty().WithMessage("ISBN is required.");
            RuleFor(x => x.ISBN).Matches("^(?:ISBN(?:-13)?:?)(?=[0-9]{13}$)([0-9]{3}-){2}[0-9]{3}[0-9X]$|^(?:ISBN(?:-10)?:?)(?=[0-9]{10}$)([0-9]{9}[0-9X])$")
                .WithMessage("ISBN must be a valid ISBN-10 or ISBN-13 format.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
        }
    }
}
