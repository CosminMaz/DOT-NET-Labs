using BookManagement.Features.Books;
using BookManagement.Persistance;
using BookManagement.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<BookManagementContext>(options =>
    options.UseSqlite("Data Source=books.db"));


// Handlers
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<GetBookByIdByPageHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetAllBooksValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DeleteBookValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetBookByIdByPageValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetBookByIdValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateBookValidator>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoints
app.MapGet("/books", async (int page, int pageSize, GetAllBooksHandler handler) =>
    await handler.Handle(new GetAllBooksRequest(page, pageSize)));

app.MapGet("/books/{id:guid}", async (Guid id, GetBookByIdHandler handler) =>
    await handler.Handle(new GetBookByIdRequest(id)));

app.MapGet("/books/by-page/{id:guid}", async (Guid id, int page, int pageSize, GetBookByIdByPageHandler handler) =>
    await handler.Handle(new GetBookByIdByPageRequest(id, page, pageSize)));

app.MapPost("/books", async (CreateBookRequest request, CreateBookHandler handler) =>
    await handler.Handle(request));

app.MapPut("/books/{id:guid}", async (Guid id, UpdateBookRequest body, UpdateBookHandler handler) =>
    await handler.Handle(body with { Id = id }));

app.MapDelete("/books/{id:guid}", async (Guid id, DeleteBookHandler handler) =>
    await handler.Handle(new DeleteBookRequest(id)));

app.Run();

