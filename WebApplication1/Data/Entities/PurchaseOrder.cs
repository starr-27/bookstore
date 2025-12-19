namespace WebApplication1.Data.Entities;

public enum PurchaseOrderStatus
{
    Created = 0,
    Received = 1,
    Cancelled = 2
}

public class PurchaseOrder
{
    public long PurchaseOrderId { get; set; }

    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Created;

    public DateTime CreatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public List<PurchaseOrderItem> Items { get; set; } = new();
}
