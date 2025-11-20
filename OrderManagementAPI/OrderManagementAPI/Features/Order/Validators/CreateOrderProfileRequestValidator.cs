using FluentValidation;
using Microsoft.Extensions.Logging;
using OrderManagementAPI.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace OrderManagementAPI.Features.Order.Validators;

public class CreateOrderProfileRequestValidator : AbstractValidator<CreateOrderProfileRequest>
{
    private readonly OrderManagementContext _context;
    private readonly ILogger<CreateOrderProfileRequestValidator> _logger;
    private readonly string[] _inappropriateWords = { "badword1", "badword2", "inappropriate" }; // Example list

    public CreateOrderProfileRequestValidator(OrderManagementContext context, ILogger<CreateOrderProfileRequestValidator> logger)
    {
        _context = context;
        _logger = logger;

        // Title Validation Rules
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("The order title is required.")
            .MinimumLength(1).WithMessage("The title must be at least 1 character long.")
            .MaximumLength(200).WithMessage("The title must not exceed 200 characters.")
            .Must(BeAppropriateTitle).WithMessage("The title contains inappropriate content.")
            .MustAsync(BeUniqueTitleForAuthor).WithMessage("An order with this title already exists for this author.");

        // Author Validation Rules
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("The author's name is required.")
            .MinimumLength(2).WithMessage("The author's name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("The author's name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-\'\.]+$").WithMessage("The author's name contains invalid characters. Only letters, spaces, hyphens, apostrophes, and dots are allowed.");

        // ISBN Validation Rules
        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("The ISBN is required.")
            .Must(BeValidIsbnFormat).WithMessage("The ISBN must be a valid 10 or 13 digit format (may contain hyphens).")
            .MustAsync(BeUniqueIsbn).WithMessage("An order with this ISBN already exists in the system.");

        // Category Validation Rules
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("The category must be a valid enum value.");

        // Price Validation Rules
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("The price must be greater than zero.")
            .LessThan(10000).WithMessage("The price must be less than $10,000.");

        // PublishedDate Validation Rules
        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("The published date cannot be in the future.")
            .GreaterThanOrEqualTo(new DateTime(1400, 1, 1)).WithMessage("The published date cannot be before the year 1400.");

        // StockQuantity Validation Rules
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("The stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("The stock quantity cannot exceed 100,000.");

        // CoverImageUrl Validation Rules
        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidImageUrl).When(x => !string.IsNullOrEmpty(x.CoverImageUrl)).WithMessage("The cover image URL must be a valid HTTP/HTTPS URL ending with an image extension (.jpg, .jpeg, .png, .gif, .webp).");

        // Business Rules Validation (placeholder for async complex rules)
        RuleFor(x => x)
            .MustAsync(PassComplexBusinessRules).WithMessage("The order failed complex business rule validation.");
    }

    private bool BeAppropriateTitle(string title)
    {
        foreach (var word in _inappropriateWords)
        {
            if (title.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Inappropriate content detected in title: {Title}", title);
                return false;
            }
        }
        return true;
    }

    private async Task<bool> BeUniqueTitleForAuthor(CreateOrderProfileRequest request, string title, CancellationToken cancellationToken)
    {
        var isUnique = !await _context.Orders.AnyAsync(
            o => o.Title == title && o.Author == request.Author,
            cancellationToken
        );
        if (!isUnique)
        {
            _logger.LogWarning("Title '{Title}' by author '{Author}' is not unique.", title, request.Author);
        }
        return isUnique;
    }

    private bool BeValidIsbnFormat(string isbn)
    {
        // Remove hyphens for validation
        var cleanIsbn = isbn.Replace("-", "");

        // ISBN-10: 9 digits + 1 checksum digit (can be X)
        // ISBN-13: 12 digits + 1 checksum digit
        var isValid = Regex.IsMatch(cleanIsbn, @"^(?:\d{9}[\dX]|\d{13})$");
        if (!isValid)
        {
            _logger.LogWarning("Invalid ISBN format: {ISBN}", isbn);
        }
        return isValid;
    }

    private async Task<bool> BeUniqueIsbn(string isbn, CancellationToken cancellationToken)
    {
        var isUnique = !await _context.Orders.AnyAsync(o => o.ISBN == isbn, cancellationToken);
        if (!isUnique)
        {
            _logger.LogWarning("ISBN '{ISBN}' is not unique.", isbn);
        }
        return isUnique;
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true; // Handled by When clause

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
            !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            _logger.LogWarning("Invalid URL scheme or format for CoverImageUrl: {URL}", url);
            return false;
        }

        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(uriResult.AbsolutePath).ToLowerInvariant();

        var isValid = imageExtensions.Contains(fileExtension);
        if (!isValid)
        {
            _logger.LogWarning("Invalid image file extension for CoverImageUrl: {URL}", url);
        }
        return isValid;
    }

    private async Task<bool> PassComplexBusinessRules(CreateOrderProfileRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing complex business rules for order: {Title}", request.Title);

        // Rule 1: Daily order addition limit check (max 500 per day)
        var ordersToday = await _context.Orders
            .CountAsync(o => o.PublishedDate.Date == DateTime.Today.Date, cancellationToken);
        if (ordersToday >= 500)
        {
            _logger.LogWarning("Business Rule Violation: Daily order addition limit exceeded. Current orders today: {Count}", ordersToday);
            return false;
        }

        // Rule 2: Technical orders minimum price check ($20.00)
        // Assuming OrderCategory.Technical exists
        if (request.Category == OrderCategory.Technical && request.Price < 20.00m)
        {
            _logger.LogWarning("Business Rule Violation: Technical order '{Title}' has price {Price} below minimum $20.00.", request.Title, request.Price);
            return false;
        }

        // Rule 3: Children's order content restrictions (check Title against restricted words)
        // Assuming OrderCategory.Children exists
        if (request.Category == OrderCategory.Children)
        {
            foreach (var word in _inappropriateWords)
            {
                if (request.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Business Rule Violation: Children's order '{Title}' contains restricted word '{RestrictedWord}'.", request.Title, word);
                    return false;
                }
            }
        }

        // Rule 4: High-value order stock limit (>$500 = max 10 stock)
        if (request.Price > 500.00m && request.StockQuantity > 10)
        {
            _logger.LogWarning("Business Rule Violation: High-value order '{Title}' (Price: {Price}) exceeds stock limit. Stock: {StockQuantity}", request.Title, request.Price, request.StockQuantity);
            return false;
        }

        _logger.LogInformation("All complex business rules passed for order: {Title}", request.Title);
        return true;
    }
}
