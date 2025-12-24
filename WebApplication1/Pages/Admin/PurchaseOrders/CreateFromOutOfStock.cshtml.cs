using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.PurchaseOrders;

public class CreateFromOutOfStockModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateFromOutOfStockModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<SupplierOption> SupplierOptions { get; private set; } = new();
    public List<RequestRow> Requests { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class SupplierOption
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = default!;
    }

    public class RequestRow
    {
        public long OutOfStockRequestId { get; set; }
        public string CustomerEmail { get; set; } = default!;
        public long? BookId { get; set; }
        public string BookTitle { get; set; } = default!;
        public uint RequestedQty { get; set; }
        public string StatusText { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }

    public class InputModel
    {
        [Required]
        public long? SupplierId { get; set; }

        [Required]
        public long[] SelectedRequestIds { get; set; } = Array.Empty<long>();

        [Range(0.01, 999999.99)]
        public decimal DefaultUnitCost { get; set; } = 1;

        public bool MarkRequestsAsOrdered { get; set; } = true;
    }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == Input.SupplierId);
        if (supplier is null)
        {
            ModelState.AddModelError(string.Empty, "供应商不存在");
            return Page();
        }

        var selected = Input.SelectedRequestIds.Distinct().ToArray();
        if (selected.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "请至少选择一条缺书记录");
            return Page();
        }

        var requests = await _db.OutOfStockRequests
            .Include(r => r.Book)
            .Where(r => selected.Contains(r.OutOfStockRequestId))
            .ToListAsync();

        var invalid = requests.Where(r => r.BookId is null || r.Book is null).ToList();
        if (invalid.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "所选记录中包含未匹配到系统图书（BookId为空）的记录，暂不支持生成采购单");
            return Page();
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var po = new PurchaseOrder
        {
            SupplierId = supplier.SupplierId,
            Status = PurchaseOrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            Items = new List<PurchaseOrderItem>()
        };

        // PurchaseOrderItem has a unique index (PurchaseOrderId, BookId),
        // so we must merge multiple out-of-stock records that target the same BookId.
        var grouped = requests
            .GroupBy(r => r.BookId!.Value)
            .Select(g => new { BookId = g.Key, Qty = g.Sum(x => (long)x.RequestedQty) })
            .ToList();

        foreach (var g in grouped)
        {
            po.Items.Add(new PurchaseOrderItem
            {
                BookId = g.BookId,
                Qty = (uint)Math.Min(uint.MaxValue, Math.Max(0, g.Qty)),
                UnitCost = Input.DefaultUnitCost
            });
        }

        if (Input.MarkRequestsAsOrdered)
        {
            foreach (var r in requests)
            {
                r.Status = OutOfStockStatus.Ordered;
                r.UpdatedAt = DateTime.UtcNow;
            }
        }

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToPage("/Admin/PurchaseOrders/Details", new { id = po.PurchaseOrderId });
    }

    private async Task LoadAsync()
    {
        SupplierOptions = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SupplierName)
            .Select(s => new SupplierOption { SupplierId = s.SupplierId, SupplierName = s.SupplierName })
            .ToListAsync();

        Requests = await _db.OutOfStockRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Where(r => r.Status == OutOfStockStatus.Submitted || r.Status == OutOfStockStatus.Processing)
            .Take(200)
            .Select(r => new RequestRow
            {
                OutOfStockRequestId = r.OutOfStockRequestId,
                CustomerEmail = _db.Users.Where(u => u.Id == r.CustomerId).Select(u => u.Email).FirstOrDefault() ?? r.CustomerId,
                BookId = r.BookId,
                BookTitle = r.BookTitle,
                RequestedQty = r.RequestedQty,
                StatusText = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }
}
