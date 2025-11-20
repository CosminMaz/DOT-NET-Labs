using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrderManagementAPI.Persistance;


namespace OrderManagementAPI.Features.Order
{
    public class CreateOrderHandler(
        OrderManagementContext context,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CreateOrderHandler> logger,
        IValidator<CreateOrderProfileRequest> validator)
        : IRequestHandler<CreateOrderProfileRequest, OrderProfileDto>
    {
        public async Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating a new order with Title: {Title}, Author: {Author}, Category: {Category}, ISBN: {ISBN}", 
                request.Title, request.Author, request.Category, request.ISBN);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var isbnExists = context.Orders.Any(o => o.ISBN == request.ISBN);
            if (isbnExists)
            {
                logger.LogWarning("Order with ISBN '{ISBN}' already exists.", request.ISBN);
                throw new System.Exception("An order with this ISBN already exists.");
            }

            var order = mapper.Map<Order>(request);

            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);

            cache.Remove("all_orders");

            logger.LogInformation("Order with ID {OrderId} created successfully.", order.Id);

            return mapper.Map<OrderProfileDto>(order);
        }
    }
}
