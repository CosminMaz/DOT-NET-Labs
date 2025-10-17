using BookManagement.Persistance;

namespace BookManagement.Features.Books;

public class GetBookByIdHandler(BookManagementContext context)
{
    private readonly BookManagementContext _context = context;
    
    public async Task<IResult> Handle(GetBookByIdRequest request)
    {
        var book = await _context.Books.FindAsync(request.Id);
        if (book == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(book);
    }
}