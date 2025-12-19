namespace WebApplication1.Data.Entities;

public class Supplier
{
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;

    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Book> Books { get; set; } = new();
}
