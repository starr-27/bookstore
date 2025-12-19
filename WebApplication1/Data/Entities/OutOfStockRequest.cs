namespace WebApplication1.Data.Entities;

public enum OutOfStockStatus
{
    Submitted = 0,
    Processing = 1,
    Ordered = 2,
    Completed = 3,
    Rejected = 4
}

public class OutOfStockRequest
{
    public long OutOfStockRequestId { get; set; }

    public string CustomerId { get; set; } = default!;
    public ApplicationUser Customer { get; set; } = default!;

    public long? BookId { get; set; }
    public Book? Book { get; set; }

    public string BookTitle { get; set; } = string.Empty;
    public uint RequestedQty { get; set; }

    public string? Note { get; set; }

    public OutOfStockStatus Status { get; set; } = OutOfStockStatus.Submitted;

    public string? AdminReply { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
