using Xunit;
using Moq;
using FluentValidation.TestHelper;
using OrderManagementAPI.Features.Order;
using OrderManagementAPI.Features.Order.Validators;
using OrderManagementAPI.Persistance;
using Microsoft.EntityFrameworkCore;
// Removed unused using Microsoft.Extensions.Logging;

namespace OrderManagementAPI.Tests;

public class CreateOrderProfileRequestValidatorUnitTests
{
    private readonly CreateOrderProfileRequestValidator _validator;
    private readonly Mock<OrderManagementContext> _mockContext;
    private readonly Mock<ILogger<CreateOrderProfileRequestValidator>> _mockLogger;

    public CreateOrderProfileRequestValidatorUnitTests()
    {
        _mockContext = new Mock<OrderManagementContext>(new DbContextOptions<OrderManagementContext>());
        _mockLogger = new Mock<ILogger<CreateOrderProfileRequestValidator>>();
        _validator = new CreateOrderProfileRequestValidator(_mockContext.Object, _mockLogger.Object);
    }

    // --- Title Validation Tests ---

    [Fact]
    public void Title_ShouldHaveError_WhenEmpty()
    {
        var request = new CreateOrderProfileRequest { Title = "", Author = "Test Author", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("The order title is required.");
    }

    [Fact]
    public void Title_ShouldHaveError_WhenTooLong()
    {
        var request = new CreateOrderProfileRequest { Title = new string('a', 201), Author = "Test Author", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("The title must not exceed 200 characters.");
    }

    [Fact]
    public void Title_ShouldHaveError_WhenInappropriate()
    {
        var request = new CreateOrderProfileRequest { Title = "This is a badword1 title", Author = "Test Author", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("The title contains inappropriate content.");
    }

    [Fact]
    public async Task Title_ShouldHaveError_WhenNotUniqueForAuthor()
    {
        // Arrange
        var existingOrder = new Order { Title = "Unique Title", Author = "John Doe", ISBN = "123", Id = Guid.NewGuid() };
        var mockDbSet = new Mock<DbSet<Order>>();
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(new[] { existingOrder }.AsQueryable().Provider);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(new[] { existingOrder }.AsQueryable().Expression);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(new[] { existingOrder }.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(new[] { existingOrder }.AsQueryable().GetEnumerator());

        _mockContext.Setup(c => c.Orders).Returns(mockDbSet.Object);

        var request = new CreateOrderProfileRequest { Title = "Unique Title", Author = "John Doe", ISBN = "1234567890" };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("An order with this title already exists for this author.");
    }

    // --- Author Validation Tests ---

    [Fact]
    public void Author_ShouldHaveError_WhenEmpty()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Author)
              .WithErrorMessage("The author's name is required.");
    }

    [Fact]
    public void Author_ShouldHaveError_WhenInvalidCharacters()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "John D@e", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Author)
              .WithErrorMessage("The author's name contains invalid characters. Only letters, spaces, hyphens, apostrophes, and dots are allowed.");
    }

    // --- ISBN Validation Tests ---

    [Fact]
    public void ISBN_ShouldHaveError_WhenEmpty()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ISBN)
              .WithErrorMessage("The ISBN is required.");
    }

    [Fact]
    public void ISBN_ShouldHaveError_WhenInvalidFormat()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "12345" }; // Too short
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ISBN)
              .WithErrorMessage("The ISBN must be a valid 10 or 13 digit format (may contain hyphens).");
    }

    [Fact]
    public async Task ISBN_ShouldHaveError_WhenNotUnique()
    {
        // Arrange
        var existingOrder = new Order { Title = "Existing Title", Author = "Existing Author", ISBN = "978-1234567890", Id = Guid.NewGuid() };
        var mockDbSet = new Mock<DbSet<Order>>();
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(new[] { existingOrder }.AsQueryable().Provider);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(new[] { existingOrder }.AsQueryable().Expression);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(new[] { existingOrder }.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(new[] { existingOrder }.AsQueryable().GetEnumerator());

        _mockContext.Setup(c => c.Orders).Returns(mockDbSet.Object);

        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "978-1234567890" };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBN)
              .WithErrorMessage("An order with this ISBN already exists in the system.");
    }

    // --- Category Validation Tests ---

    [Fact]
    public void Category_ShouldHaveError_WhenInvalidEnumValue()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Category = (OrderCategory)999 }; // Invalid enum value
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Category)
              .WithErrorMessage("The category must be a valid enum value.");
    }

    // --- Price Validation Tests ---

    [Fact]
    public void Price_ShouldHaveError_WhenZero()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Price = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("The price must be greater than zero.");
    }

    [Fact]
    public void Price_ShouldHaveError_WhenTooHigh()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Price = 10000.01m };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("The price must be less than $10,000.");
    }

    // --- PublishedDate Validation Tests ---

    [Fact]
    public void PublishedDate_ShouldHaveError_WhenInFuture()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", PublishedDate = DateTime.Today.AddDays(1) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PublishedDate)
              .WithErrorMessage("The published date cannot be in the future.");
    }

    // --- StockQuantity Validation Tests ---

    [Fact]
    public void StockQuantity_ShouldHaveError_WhenNegative()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", StockQuantity = -1 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
              .WithErrorMessage("The stock quantity cannot be negative.");
    }

    // --- CoverImageUrl Validation Tests ---

    [Fact]
    public void CoverImageUrl_ShouldHaveError_WhenInvalidUrlFormat()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", CoverImageUrl = "not-a-url" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverImageUrl)
              .WithErrorMessage("The cover image URL must be a valid HTTP/HTTPS URL ending with an image extension (.jpg, .jpeg, .png, .gif, .webp).");
    }

    [Fact]
    public void CoverImageUrl_ShouldHaveError_WhenInvalidExtension()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", CoverImageUrl = "http://example.com/image.txt" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverImageUrl)
              .WithErrorMessage("The cover image URL must be a valid HTTP/HTTPS URL ending with an image extension (.jpg, .jpeg, .png, .gif, .webp).");
    }

    // --- Conditional Validation Tests ---

    [Fact]
    public void TechnicalOrder_Price_ShouldHaveError_WhenBelowMinimum()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Category = OrderCategory.Technical, Price = 19.99m };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("Technical orders must have a price of at least $20.00.");
    }

    [Fact]
    public void TechnicalOrder_Title_ShouldHaveError_WhenNoTechnicalKeywords()
    {
        var request = new CreateOrderProfileRequest { Category = OrderCategory.Technical, Title = "A simple story", Author = "Test Author", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Technical orders must contain technical keywords in the title.");
    }

    [Fact]
    public void TechnicalOrder_PublishedDate_ShouldHaveError_WhenTooOld()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Category = OrderCategory.Technical, PublishedDate = DateTime.UtcNow.AddYears(-6) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PublishedDate)
              .WithErrorMessage("Technical orders must be published within the last 5 years.");
    }

    [Fact]
    public void ChildrensOrder_Price_ShouldHaveError_WhenAboveMaximum()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Category = OrderCategory.Children, Price = 50.01m };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("Children's orders must have a price of no more than $50.00.");
    }

    [Fact]
    public void ChildrensOrder_Title_ShouldHaveError_WhenInappropriate()
    {
        var request = new CreateOrderProfileRequest { Category = OrderCategory.Children, Title = "Children's badword1 book", Author = "Test Author", ISBN = "1234567890" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("The title contains inappropriate content.");
    }

    [Fact]
    public void FictionOrder_Author_ShouldHaveError_WhenTooShort()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", ISBN = "1234567890", Category = OrderCategory.Fiction, Author = "A.B." }; // 4 chars
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Author)
              .WithErrorMessage("Fiction orders require the author's name to be at least 5 characters long.");
    }

    [Fact]
    public void ExpensiveOrder_StockQuantity_ShouldHaveError_WhenTooHigh()
    {
        var request = new CreateOrderProfileRequest { Title = "Test Title", Author = "Test Author", ISBN = "1234567890", Price = 100.01m, StockQuantity = 21 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
              .WithErrorMessage("Expensive orders (>$100) must have a limited stock (≤20 units).");
    }

    // --- PassComplexBusinessRules (Daily Limit) Test ---
    // This rule is async and interacts with the DB, so it's more of an integration test.
    // However, we can mock the context to simulate the count.
    [Fact]
    public async Task PassComplexBusinessRules_ShouldFail_WhenDailyLimitExceeded()
    {
        // Arrange
        var mockDbSet = new Mock<DbSet<Order>>();
        // Simulate 500 orders already existing today
        var ordersToday = Enumerable.Range(0, 500).Select(i => new Order { Id = Guid.NewGuid(), Title = $"Title {i}", Author = $"Author {i}", ISBN = $"ISBN{i}", PublishedDate = DateTime.Today }).AsQueryable();
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(ordersToday.Provider);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(ordersToday.Expression);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(ordersToday.ElementType);
        mockDbSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(ordersToday.GetEnumerator());

        _mockContext.Setup(c => c.Orders).Returns(mockDbSet.Object);

        var request = new CreateOrderProfileRequest { Title = "Test", Author = "Test", ISBN = "1234567890", Category = OrderCategory.Fiction, Price = 10, PublishedDate = DateTime.Today, StockQuantity = 1 };

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("The order failed complex business rule validation.");

        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<string>(o => o.Contains("Daily order addition limit exceeded")), // Removed explicit cast
                It.IsAny<Exception?>(),
                It.IsAny<Func<string, Exception?, string>>()),
            Times.Once);
    }
}
