namespace WebApplication1.Data.Entities;

public class Author
{
    public long AuthorId { get; set; }
    public string AuthorName { get; set; } = default!;

    public List<BookAuthor> Books { get; set; } = new();
}
