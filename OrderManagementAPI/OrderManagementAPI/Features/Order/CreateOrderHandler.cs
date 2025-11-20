using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Persistance;
using OrderManagementAPI.Common;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OrderManagementAPI.Features.Order
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderProfileRequest, OrderProfileDto>
    {
        private readonly OrderManagementContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateOrderHandler> _logger;

        public CreateOrderHandler(OrderManagementContext context, IMapper mapper, ILogger<CreateOrderHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(LogEvents.OrderCreationStarted, "Creating a new order - Title: {Title}, Author: {Author}, Category: {Category}, ISBN: {ISBN}", request.Title, request.Author, request.Category, request.ISBN);

            // Time ISBN uniqueness check
            var isbnValidationStopwatch = Stopwatch.StartNew();
            if (await _context.Orders.AnyAsync(o => o.ISBN == request.ISBN, cancellationToken))
            {
                isbnValidationStopwatch.Stop();
                if (MetricsContext.CurrentMetrics != null)
                {
                    MetricsContext.CurrentMetrics = MetricsContext.CurrentMetrics with
                    {
                        ValidationDuration = MetricsContext.CurrentMetrics.ValidationDuration.Add(isbnValidationStopwatch.Elapsed)
                    };
                }
                _logger.LogWarning(LogEvents.OrderValidationFailed, "Order with ISBN: {ISBN} already exists. ISBN validation took {DurationMs}ms.", request.ISBN, isbnValidationStopwatch.ElapsedMilliseconds);
                throw new Exception("Order with this ISBN already exists.");
            }
            isbnValidationStopwatch.Stop();
            if (MetricsContext.CurrentMetrics != null)
            {
                MetricsContext.CurrentMetrics = MetricsContext.CurrentMetrics with
                {
                    ValidationDuration = MetricsContext.CurrentMetrics.ValidationDuration.Add(isbnValidationStopwatch.Elapsed)
                };
            }
            _logger.LogInformation(LogEvents.ISBNValidationPerformed, "ISBN uniqueness check for {ISBN} took {DurationMs}ms.", request.ISBN, isbnValidationStopwatch.ElapsedMilliseconds);


            var order = _mapper.Map<Order>(request);

            _logger.LogInformation(LogEvents.DatabaseOperationStarted, "Starting database operation to save order with ISBN: {ISBN}", request.ISBN);
            var dbStopwatch = Stopwatch.StartNew();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);
            dbStopwatch.Stop();
            _logger.LogInformation(LogEvents.DatabaseOperationCompleted, "Database operation completed for OrderId: {OrderId} in {DurationMs}ms", order.Id, dbStopwatch.ElapsedMilliseconds);

            if (MetricsContext.CurrentMetrics != null)
            {
                MetricsContext.CurrentMetrics = MetricsContext.CurrentMetrics with
                {
                    DatabaseSaveDuration = dbStopwatch.Elapsed
                };
            }

            // Log cache operation
            _logger.LogInformation(LogEvents.CacheOperationPerformed, "Cache operation performed for key: {CacheKey}", "all_orders");

            _logger.LogInformation(LogEvents.OrderCreationCompleted, "Order created successfully with ID: {OrderId}", order.Id);

            var orderDto = _mapper.Map<OrderProfileDto>(order);

            return orderDto;
        }
    }
}
