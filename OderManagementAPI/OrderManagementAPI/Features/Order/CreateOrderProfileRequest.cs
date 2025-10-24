using System;

namespace OrderManagementAPI.Features.Order
{
    public record CreateOrderProfileRequest(
        string Title,
        string Author,
        string ISBN,
        OrderCategory Category,
        decimal Price,
        DateTime PublishedDate,
        string? CoverImageUrl,
        int StockQuantity = 1);
}
