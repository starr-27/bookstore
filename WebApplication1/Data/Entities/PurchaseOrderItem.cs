namespace WebApplication1.Data.Entities;

public class PurchaseOrderItem
{
    public long PurchaseOrderItemId { get; set; }

    public long PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = default!;

    public long BookId { get; set; }
    public Book Book { get; set; } = default!;

    public uint Qty { get; set; }
    public decimal UnitCost { get; set; }
}
