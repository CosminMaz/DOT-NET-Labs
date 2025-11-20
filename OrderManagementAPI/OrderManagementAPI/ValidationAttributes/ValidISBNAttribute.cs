using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OrderManagementAPI.ValidationAttributes;

public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
{
    private const string IsbnPattern = @"^(?:\d{9}[\dX]|\d{13})$";

    public ValidISBNAttribute()
    {
        ErrorMessage = "The ISBN must be a valid 10 or 13 digit format (may contain hyphens or spaces).";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string isbn)
        {
            return ValidationResult.Success; // Not a string, let other validators handle null/empty
        }

        var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

        if (string.IsNullOrEmpty(cleanIsbn))
        {
            return ValidationResult.Success; // Empty after cleaning, let other validators handle NotEmpty
        }

        if (!Regex.IsMatch(cleanIsbn, IsbnPattern))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-isbn", ErrorMessage);
        MergeAttribute(context.Attributes, "data-val-isbn-pattern", IsbnPattern); // Pass pattern for client-side regex
    }

    private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }
        attributes.Add(key, value);
        return true;
    }
}
