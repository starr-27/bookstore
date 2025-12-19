namespace WebApplication1.Data.Entities;

public class Book
{
    public long BookId { get; set; }
    public string BookNo { get; set; } = default!;
    public string Title { get; set; } = default!;

    public long? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    public decimal Price { get; set; }
    public uint StockQty { get; set; }

    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<BookAuthor> Authors { get; set; } = new();
    public List<BookKeyword> Keywords { get; set; } = new();
}
