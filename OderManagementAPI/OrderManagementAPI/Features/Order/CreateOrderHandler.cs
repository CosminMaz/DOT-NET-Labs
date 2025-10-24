using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrderManagementAPI.Persistance;


namespace OrderManagementAPI.Features.Order
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderProfileRequest, OrderProfileDto>
    {
        private readonly OrderManagementContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CreateOrderHandler> _logger;
        private readonly IValidator<CreateOrderProfileRequest> _validator;

        public CreateOrderHandler(OrderManagementContext context, IMapper mapper, IMemoryCache cache, ILogger<CreateOrderHandler> logger, IValidator<CreateOrderProfileRequest> validator)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _validator = validator;
        }

        public async Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new order with Title: {Title}, Author: {Author}, Category: {Category}, ISBN: {ISBN}", 
                request.Title, request.Author, request.Category, request.ISBN);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var isbnExists = _context.Orders.Any(o => o.ISBN == request.ISBN);
            if (isbnExists)
            {
                _logger.LogWarning("Order with ISBN '{ISBN}' already exists.", request.ISBN);
                throw new System.Exception("An order with this ISBN already exists.");
            }

            var order = _mapper.Map<Order>(request);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            _cache.Remove("all_orders");

            _logger.LogInformation("Order with ID {OrderId} created successfully.", order.Id);

            return _mapper.Map<OrderProfileDto>(order);
        }
    }
}
