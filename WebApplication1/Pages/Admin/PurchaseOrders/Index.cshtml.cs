using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin.PurchaseOrders;

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
        public long PurchaseOrderId { get; set; }
        public string SupplierName { get; set; } = default!;
        public string StatusText { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public int ItemCount { get; set; }
    }

    public async Task OnGetAsync()
    {
        Rows = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .Take(200)
            .Select(p => new Row
            {
                PurchaseOrderId = p.PurchaseOrderId,
                SupplierName = p.Supplier.SupplierName,
                StatusText = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                ReceivedAt = p.ReceivedAt,
                ItemCount = p.Items.Count
            })
            .ToListAsync();
    }
}
