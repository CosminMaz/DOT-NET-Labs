using OrderManagementAPI.Features.Order;

namespace OrderManagementAPI
{
    public record OrderCreationMetrics(
        string OperationId,
        string OrderTitle,
        string ISBN,
        OrderCategory Category,
        TimeSpan ValidationDuration,
        TimeSpan DatabaseSaveDuration,
        TimeSpan TotalDuration,
        bool Success,
        string? ErrorReason,
        DateTime OperationStartTime
    );
}