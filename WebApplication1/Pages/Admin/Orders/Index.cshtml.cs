using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Orders;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string? Status { get; set; }

    public List<(string Value, string Text)> StatusOptions { get; } =
    [
        (nameof(OrderStatus.Created), nameof(OrderStatus.Created)),
        (nameof(OrderStatus.Paid), nameof(OrderStatus.Paid)),
        (nameof(OrderStatus.Packed), nameof(OrderStatus.Packed)),
        (nameof(OrderStatus.Shipped), nameof(OrderStatus.Shipped)),
        (nameof(OrderStatus.Completed), nameof(OrderStatus.Completed)),
        (nameof(OrderStatus.Cancelled), nameof(OrderStatus.Cancelled))
    ];

    public List<OrderRow> Orders { get; private set; } = new();

    public class OrderRow
    {
        public long OrderId { get; set; }
        public string CustomerId { get; set; } = default!;
        public string? CustomerEmail { get; set; }
        public string StatusText { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public async Task OnGetAsync(string? status)
    {
        Status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();

        var query = _db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(Status) && Enum.TryParse<OrderStatus>(Status, out var parsed))
        {
            query = query.Where(o => o.OrderStatus == parsed);
        }

        Orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Take(200)
            .Select(o => new OrderRow
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                CustomerEmail = _db.Users.Where(u => u.Id == o.CustomerId).Select(u => u.Email).FirstOrDefault(),
                StatusText = o.OrderStatus.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }
}
