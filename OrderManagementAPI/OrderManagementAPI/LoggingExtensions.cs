using Microsoft.Extensions.Logging;
using OrderManagementAPI.Features.Order;

namespace OrderManagementAPI
{
    public static partial class LoggingExtensions
    {
        [LoggerMessage(
            EventId = LogEvents.OrderCreationCompleted,
            Level = LogLevel.Information,
            Message = "Order Creation Metrics - Title: {OrderTitle}, ISBN: {ISBN}, Category: {Category}, Validation Duration: {ValidationDurationMs}ms, Database Save Duration: {DatabaseSaveDurationMs}ms, Total Duration: {TotalDurationMs}ms, Success: {Success}, Error Reason: {ErrorReason}, Operation Start Time: {OperationStartTime}"
        )]
        public static partial void LogOrderCreationMetrics(
            this ILogger logger,
            string OrderTitle,
            string ISBN,
            OrderCategory Category,
            long ValidationDurationMs,
            long DatabaseSaveDurationMs,
            long TotalDurationMs,
            bool Success,
            string? ErrorReason,
            DateTime OperationStartTime
        );
    }
}