using BookManagement.Persistance;
using BookManagement.Validators;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Features.Books;

public class GetBookByIdByPageHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;

    public async Task<IResult> Handle(GetBookByIdByPageRequest request)
    {
        var validator = new GetBookByIdByPageValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var skip = (request.Page - 1) * request.PageSize;

        // Apply deterministic ordering for pagination
        var pageBooks = await _context.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .ThenBy(b => b.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        var book = pageBooks.FirstOrDefault(b => b.Id == request.Id);
        if (book is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(book);
    }
}
