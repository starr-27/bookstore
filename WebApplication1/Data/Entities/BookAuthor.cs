namespace WebApplication1.Data.Entities;

public class BookAuthor
{
    public long BookId { get; set; }
    public Book Book { get; set; } = default!;

    public long AuthorId { get; set; }
    public Author Author { get; set; } = default!;

    public byte AuthorOrder { get; set; }
}
