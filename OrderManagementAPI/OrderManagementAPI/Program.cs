using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Common.Behaviors;
using OrderManagementAPI.Common.Middleware;
using OrderManagementAPI.Features.Order;
using OrderManagementAPI.Features.Order.Validators;
using OrderManagementAPI.Persistance;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OrderManagementAPI;
using OrderManagementAPI.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<OrderManagementContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderProfileRequestValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderManagementContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Add CorrelationMiddleware to the pipeline
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderProfileRequest request, IMediator mediator, ILogger<Program> logger, HttpContext httpContext) =>
{
    // Retrieve CorrelationId from HttpContext.Items, set by CorrelationMiddleware
    var operationId = httpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString()[..8];
    
    using (logger.BeginScope(new Dictionary<string, object> { { "OperationId", operationId } }))
    {
        var stopwatch = Stopwatch.StartNew();
        var operationStartTime = DateTime.UtcNow; // Capture start time
        logger.LogInformation(LogEvents.OrderCreationStarted, "Order creation started.");

        MetricsContext.CurrentMetrics = new OrderCreationMetrics(
            OperationId: operationId,
            OrderTitle: request.Title,
            ISBN: request.ISBN,
            Category: request.Category,
            ValidationDuration: TimeSpan.Zero,
            DatabaseSaveDuration: TimeSpan.Zero,
            TotalDuration: TimeSpan.Zero,
            Success: false,
            ErrorReason: null,
            OperationStartTime: operationStartTime // Set the start time
        );

        try
        {
            var orderDto = await mediator.Send(request);
            stopwatch.Stop();
            var finalMetrics = MetricsContext.CurrentMetrics! with
            {
                TotalDuration = stopwatch.Elapsed,
                Success = true
            };
            logger.LogOrderCreationMetrics(
                finalMetrics.OrderTitle,
                finalMetrics.ISBN,
                finalMetrics.Category,
                (long)finalMetrics.ValidationDuration.TotalMilliseconds,
                (long)finalMetrics.DatabaseSaveDuration.TotalMilliseconds,
                (long)finalMetrics.TotalDuration.TotalMilliseconds,
                finalMetrics.Success,
                finalMetrics.ErrorReason,
                finalMetrics.OperationStartTime // Pass the start time
            );
            return Results.Created($"/orders/{orderDto.Id}", orderDto);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var finalMetrics = MetricsContext.CurrentMetrics! with
            {
                TotalDuration = stopwatch.Elapsed,
                Success = false,
                ErrorReason = ex.Message
            };
            logger.LogError(ex, "An error occurred during order creation.");
            logger.LogOrderCreationMetrics(
                finalMetrics.OrderTitle,
                finalMetrics.ISBN,
                finalMetrics.Category,
                (long)finalMetrics.ValidationDuration.TotalMilliseconds,
                (long)finalMetrics.DatabaseSaveDuration.TotalMilliseconds,
                (long)finalMetrics.TotalDuration.TotalMilliseconds,
                finalMetrics.Success,
                finalMetrics.ErrorReason,
                finalMetrics.OperationStartTime // Pass the start time
            );
            throw;
        }
        finally
        {
            MetricsContext.CurrentMetrics = null; // Clear the context
        }
    }
});

app.Run();
