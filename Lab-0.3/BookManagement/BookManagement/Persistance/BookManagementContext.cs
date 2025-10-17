using BookManagement.Features.Books;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Persistance;

public class BookManagementContext: DbContext
{
    public BookManagementContext(DbContextOptions<BookManagementContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
}