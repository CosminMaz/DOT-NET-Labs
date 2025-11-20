using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OrderManagementAPI.ValidationAttributes;

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public PriceRangeAttribute(double minPrice, double maxPrice)
    {
        _minPrice = (decimal)minPrice;
        _maxPrice = (decimal)maxPrice;
        ErrorMessage = GenerateErrorMessage();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Let [Required] handle nulls
        }

        if (value is not decimal price)
        {
            return new ValidationResult("Invalid price type.");
        }

        if (price < _minPrice || price > _maxPrice)
        {
            return new ValidationResult(GenerateErrorMessage());
        }

        return ValidationResult.Success;
    }

    private string GenerateErrorMessage()
    {
        return $"The price must be between {_minPrice.ToString("C", CultureInfo.CurrentCulture)} and {_maxPrice.ToString("C", CultureInfo.CurrentCulture)}.";
    }
}
