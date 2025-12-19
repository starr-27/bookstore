namespace WebApplication1.Data.Entities;

public class BookKeyword
{
    public long BookId { get; set; }
    public Book Book { get; set; } = default!;

    public long KeywordId { get; set; }
    public Keyword Keyword { get; set; } = default!;
}
