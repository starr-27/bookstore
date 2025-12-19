namespace WebApplication1.Data.Entities;

public enum OrderStatus
{
    Created = 0,
    Paid = 1,
    Packed = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5
}

public class Order
{
    public long OrderId { get; set; }
    public string CustomerId { get; set; } = default!;

    public OrderStatus OrderStatus { get; set; } = OrderStatus.Created;
    public decimal TotalAmount { get; set; }

    public string ReceiverName { get; set; } = default!;
    public string ReceiverPhone { get; set; } = default!;
    public string ReceiverAddr { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
    public Shipment? Shipment { get; set; }
}
