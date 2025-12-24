namespace WebApplication1.Data.Entities;

public class Book
{
    public long BookId { get; set; }
    public string BookNo { get; set; } = default!;
    public string Title { get; set; } = default!;

    /// <summary>
    /// Volume identifier within the same BookNo (e.g. "Vol.1", "ио╡А", "A").
    /// When null/empty, it represents the single-volume edition.
    /// </summary>
    public string? VolumeNo { get; set; }

    public long? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    public decimal Price { get; set; }
    public uint StockQty { get; set; }

    public long? CategoryId { get; set; }
    public Category? Category { get; set; }

    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// Whether this title is published in supplier's catalog (when SupplierId is set).
    /// </summary>
    public bool SupplierCatalogEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<BookAuthor> Authors { get; set; } = new();
    public List<BookKeyword> Keywords { get; set; } = new();
}
