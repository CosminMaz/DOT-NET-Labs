using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using OrderManagementAPI.Features.Order; // Corrected using statement

namespace OrderManagementAPI.ValidationAttributes;

public class OrderCategoryAttribute : ValidationAttribute
{
    private readonly string[] _allowedCategories;

    public OrderCategoryAttribute(params string[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
        ErrorMessage = GenerateErrorMessage();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Let [Required] handle nulls
        }

        // Check if the value is an enum of type OrderCategory
        if (value is not OrderCategory categoryEnum)
        {
            // If it's not an OrderCategory enum, it's an invalid type for this attribute
            return new ValidationResult("The category must be a valid OrderCategory enum value.");
        }

        // Convert the enum value to its string representation for comparison
        var categoryString = categoryEnum.ToString();

        if (!_allowedCategories.Contains(categoryString, StringComparer.OrdinalIgnoreCase))
        {
            return new ValidationResult(GenerateErrorMessage());
        }

        return ValidationResult.Success;
    }

    private string GenerateErrorMessage()
    {
        return $"The category must be one of the following: {string.Join(", ", _allowedCategories)}.";
    }
}
