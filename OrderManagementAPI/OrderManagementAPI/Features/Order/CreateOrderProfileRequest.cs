using MediatR;
using System.ComponentModel.DataAnnotations;
using OrderManagementAPI.ValidationAttributes; // Add this using statement
using OrderManagementAPI.Features.Order; // Corrected using statement for OrderCategory

namespace OrderManagementAPI.Features.Order;

public class CreateOrderProfileRequest : IRequest<OrderProfileDto>
{
    [Required(ErrorMessage = "The order title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "The title must be between 1 and 200 characters long.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "The author's name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The author's name must be between 2 and 100 characters long.")]
    public string Author { get; set; }

    [Required(ErrorMessage = "The ISBN is required.")]
    [ValidISBNAttribute] // Apply the custom ISBN validation attribute
    public string ISBN { get; set; }

    [Required(ErrorMessage = "The category is required.")]
    // Assuming common categories for demonstration. Adjust as needed.
    [OrderCategory("Fiction", "NonFiction", "Technical", "Children", "Science", "History")]
    public OrderCategory Category { get; set; }

    [PriceRange(0.01, 9999.99)] // Apply the custom PriceRange validation attribute
    public decimal Price { get; set; }

    [Required(ErrorMessage = "The published date is required.")]
    public DateTime PublishedDate { get; set; }

    public string? CoverImageUrl { get; set; }

    [Range(0, 100000, ErrorMessage = "The stock quantity must be between 0 and 100,000.")]
    public int StockQuantity { get; set; } = 1;
}
