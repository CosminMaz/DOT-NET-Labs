
using FluentValidation;
using OrderManagementAPI.Features.Order;
using System;
using System.Linq;

namespace OrderManagementAPI.Features.Order.Validators
{
    public class CreateOrderProfileRequestValidator : AbstractValidator<CreateOrderProfileRequest>
    {
        public CreateOrderProfileRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.")
                .MaximumLength(100).WithMessage("Author must not exceed 100 characters.");

            RuleFor(x => x.ISBN)
                .Must(BeAValidIsbn).WithMessage("'{PropertyValue}' is not a valid ISBN-10 or ISBN-13.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("A valid category is required.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.PublishedDate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Published date cannot be in the future.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
        }

        private bool BeAValidIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;

            isbn = isbn.Replace("-", "").Replace(" ", "").ToUpper();

            if (isbn.Length == 10)
            {
                if (!isbn.Substring(0, 9).All(char.IsDigit)) return false;

                int sum = 0;
                for (int i = 0; i < 9; i++)
                {
                    sum += (i + 1) * (isbn[i] - '0');
                }

                char lastChar = isbn[9];
                int checksum = sum % 11;

                if (lastChar == 'X')
                {
                    return checksum == 10;
                }
                
                if (char.IsDigit(lastChar))
                {
                    return checksum == (lastChar - '0');
                }

                return false;
            }
            else if (isbn.Length == 13)
            {
                if (!isbn.All(char.IsDigit)) return false;

                int sum = 0;
                for (int i = 0; i < 12; i++)
                {
                    sum += (isbn[i] - '0') * ((i % 2 == 0) ? 1 : 3);
                }

                int checksum = (10 - (sum % 10)) % 10;
                return checksum == (isbn[12] - '0');
            }

            return false;
        }
    }
}
