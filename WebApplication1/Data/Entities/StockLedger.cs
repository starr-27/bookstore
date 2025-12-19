namespace WebApplication1.Data.Entities;

public enum StockChangeType
{
    PurchaseIn = 0,
    ManualAdjust = 1,
    SaleOut = 2
}

public class StockLedger
{
    public long StockLedgerId { get; set; }
    public long BookId { get; set; }
    public Book Book { get; set; } = default!;

    public StockChangeType ChangeType { get; set; }

    public int QtyChange { get; set; }
    public uint QtyAfter { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
