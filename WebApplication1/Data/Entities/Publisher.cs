namespace WebApplication1.Data.Entities;

public class Publisher
{
    public long PublisherId { get; set; }
    public string PublisherName { get; set; } = default!;

    public List<Book> Books { get; set; } = new();
}
