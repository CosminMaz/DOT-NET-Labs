using BookManagement.Persistance;
using BookManagement.Validators;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class GetAllBooksHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;
    
    public async Task<IResult> Handle(GetAllBooksRequest request)
    {
        var validator = new GetAllBooksValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var skip = (request.Page - 1) * request.PageSize;
        var books = await _context.Books
            .AsNoTracking()
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();
        return Results.Ok(books);
    }
}