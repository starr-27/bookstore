namespace WebApplication1.Data.Entities;

public class Shipment
{
    public long ShipmentId { get; set; }
    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public string? Carrier { get; set; }
    public string? TrackingNo { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
