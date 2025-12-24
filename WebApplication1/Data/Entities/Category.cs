namespace WebApplication1.Data.Entities;

public class Category
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;

    public List<Book> Books { get; set; } = new();
}
