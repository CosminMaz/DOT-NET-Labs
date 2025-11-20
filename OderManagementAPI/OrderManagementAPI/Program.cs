
using FluentValidation;
using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Features.Order;
using OrderManagementAPI.Persistance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderManagementContext>(options =>
    options.UseInMemoryDatabase("OrderManagement"));

// Register MediatR and FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapPost("/api/orders", async (IMediator mediator, CreateOrderProfileRequest request) =>
{
    var order = await mediator.Send(request);
    return Results.Created($"/api/orders/{order.Id}", order);
})
.WithName("CreateOrder")
.WithTags("Orders");

app.Run();
