namespace WebApplication1.Data.Entities;

public class OrderItem
{
    public long OrderItemId { get; set; }

    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public long BookId { get; set; }
    public Book Book { get; set; } = default!;

    public uint Qty { get; set; }
    public decimal UnitPrice { get; set; }
}
