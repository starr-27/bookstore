using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin.OutOfStock;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Row> Rows { get; private set; } = new();

    public class Row
    {
        public long OutOfStockRequestId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerEmail { get; set; } = default!;
        public string BookTitle { get; set; } = default!;
        public uint RequestedQty { get; set; }
        public string StatusText { get; set; } = default!;
    }

    public async Task OnGetAsync()
    {
        Rows = await _db.OutOfStockRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(200)
            .Select(r => new Row
            {
                OutOfStockRequestId = r.OutOfStockRequestId,
                CreatedAt = r.CreatedAt,
                CustomerEmail = _db.Users.Where(u => u.Id == r.CustomerId).Select(u => u.Email).FirstOrDefault() ?? r.CustomerId,
                BookTitle = r.BookTitle,
                RequestedQty = r.RequestedQty,
                StatusText = r.Status.ToString(),
            })
            .ToListAsync();
    }
}
