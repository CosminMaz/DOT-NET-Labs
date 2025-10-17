namespace BookManagement.Features.Books;

public record GetBookByIdByPageRequest(Guid Id, int Page = 1, int PageSize = 10);
