using Xunit;
using Moq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagementAPI.Persistance;
using OrderManagementAPI.Features.Order;
using OrderManagementAPI.Features.Order.Mappings;
using Microsoft.Extensions.Caching.Memory; // For IMemoryCache

namespace OrderManagementAPI.Tests;

public class CreateOrderHandlerIntegrationTests : IDisposable
{
    private readonly OrderManagementContext _context;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;
    private readonly IMemoryCache _memoryCache; // Although not directly used by handler, good to set up

    public CreateOrderHandlerIntegrationTests()
    {
        // Set up in-memory database with unique name
        var options = new DbContextOptionsBuilder<OrderManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new OrderManagementContext(options);

        // Configure AutoMapper with both order profiles
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdvancedOrderMappingProfile>();
            // If there's another profile, add it here: cfg.AddProfile<AnotherOrderMappingProfile>();
        });
        var mapper = mapperConfiguration.CreateMapper();

        // Set up memory cache (mocking if needed, but a real one is fine for integration)
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Mock ILogger<CreateOrderHandler>
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        // Create handler instance with all dependencies
        _handler = new CreateOrderHandler(_context, mapper, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _memoryCache.Dispose();
    }

    [Fact]
    public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
    {
        // Arrange
        var request = new CreateOrderProfileRequest
        {
            Title = "Advanced C# Programming",
            Author = "John Doe",
            ISBN = "978-1234567890",
            Category = OrderCategory.Technical,
            Price = 35.50m,
            PublishedDate = DateTime.UtcNow.AddYears(-2), // Within last 5 years
            CoverImageUrl = "http://example.com/tech_cover.jpg",
            StockQuantity = 15
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert: Verify Created result type
        Assert.NotNull(result);
        Assert.IsType<OrderProfileDto>(result);

        // Assert: Check CategoryDisplayName = "Technical & Professional"
        Assert.Equal("Technical & Professional", result.CategoryDisplayName);

        // Assert: Check AuthorInitials for two-name author
        Assert.Equal("J.D.", result.AuthorInitials);

        // Assert: Check PublishedAge calculation
        var expectedPublishedAge = (DateTime.UtcNow.Year - request.PublishedDate.Year);
        if (DateTime.UtcNow.Month < request.PublishedDate.Month || (DateTime.UtcNow.Month == request.PublishedDate.Month && DateTime.UtcNow.Day < request.PublishedDate.Day))
        {
            expectedPublishedAge--;
        }
        Assert.Equal(expectedPublishedAge, int.Parse(result.PublishedAge));

        // Assert: Check FormattedPrice starts with currency symbol (e.g., "$")
        Assert.StartsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, result.FormattedPrice);
        Assert.Contains("35.50", result.FormattedPrice); // Check the value part

        // Assert: Check AvailabilityStatus based on stock
        Assert.Equal("In Stock", result.AvailabilityStatus);

        // Assert: Verify OrderCreationStarted log called once
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == LogEvents.OrderCreationStarted),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
    {
        // Arrange: Create existing order in database with specific ISBN
        var existingISBN = "978-0000000001";
        _context.Orders.Add(new Order
        {
            Id = Guid.NewGuid(),
            Title = "Existing Book",
            Author = "Existing Author",
            ISBN = existingISBN,
            Category = OrderCategory.Fiction,
            Price = 10.00m,
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            CreatedAt = DateTime.UtcNow,
            IsAvailable = true,
            StockQuantity = 1
        });
        await _context.SaveChangesAsync();

        // Arrange: Create request with same ISBN
        var request = new CreateOrderProfileRequest
        {
            Title = "New Book",
            Author = "New Author",
            ISBN = existingISBN, // Duplicate ISBN
            Category = OrderCategory.Fiction,
            Price = 20.00m,
            PublishedDate = DateTime.UtcNow,
            StockQuantity = 1
        };

        // Act & Assert: Verify ValidationException thrown
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));

        // Assert: Check exception message contains "already exists"
        Assert.Contains("already exists", exception.Message);

        // Assert: Verify OrderValidationFailed log called once
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.Is<EventId>(e => e.Id == LogEvents.OrderValidationFailed),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
    {
        // Arrange: Create valid Children's order request
        var request = new CreateOrderProfileRequest
        {
            Title = "The Little Bear's Adventure",
            Author = "A.N. Author",
            ISBN = "978-9876543210",
            Category = OrderCategory.Children,
            Price = 25.00m, // Original price
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            CoverImageUrl = "http://example.com/children_cover.jpg", // Should be nullified
            StockQuantity = 5
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert: Check CategoryDisplayName = "Children's Orders"
        Assert.Equal("Children's Orders", result.CategoryDisplayName);

        // Assert: Check Price has 10% discount applied (25.00 * 0.90 = 22.50)
        // Note: The mapping profile needs to apply this discount.
        // Assuming PriceFormatterResolver or another resolver handles this.
        // If not, this assertion will fail and the mapping profile needs adjustment.
        Assert.Contains("22.50", result.FormattedPrice); // Checking formatted string for discounted price

        // Assert: Check CoverImageUrl is null (content filtering)
        Assert.Null(result.CoverImageUrl);
    }
}
