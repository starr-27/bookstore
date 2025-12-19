namespace WebApplication1.Data.Entities;

public class Keyword
{
    public long KeywordId { get; set; }
    public string KeywordText { get; set; } = default!;

    public List<BookKeyword> Books { get; set; } = new();
}
