using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.PurchaseOrders;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public PurchaseOrderVm? Vm { get; private set; }

    [BindProperty]
    public ReceiveInputModel ReceiveInput { get; set; } = new();

    public class PurchaseOrderVm
    {
        public long PurchaseOrderId { get; set; }
        public string SupplierName { get; set; } = default!;
        public PurchaseOrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public List<ItemVm> Items { get; set; } = new();
    }

    public class ItemVm
    {
        public long PurchaseOrderItemId { get; set; }
        public long BookId { get; set; }
        public string BookNo { get; set; } = default!;
        public string? VolumeNo { get; set; }
        public string Title { get; set; } = default!;
        public uint Qty { get; set; }
        public decimal UnitCost { get; set; }
        public uint StockQty { get; set; }
    }

    public class ReceiveInputModel
    {
        public bool CloseOutOfStockRequests { get; set; } = true;

        [StringLength(255)]
        public string? Note { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        await LoadAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostReceiveAsync(long id)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(id);
            return Page();
        }

        var po = await _db.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (po is null)
        {
            return NotFound();
        }

        if (po.Status != PurchaseOrderStatus.Created)
        {
            return RedirectToPage(new { id });
        }

        var bookIds = po.Items.Select(i => i.BookId).Distinct().ToArray();

        await using var tx = await _db.Database.BeginTransactionAsync();

        foreach (var item in po.Items)
        {
            var book = await _db.Books.FirstAsync(b => b.BookId == item.BookId);
            book.StockQty = checked(book.StockQty + item.Qty);
            book.UpdatedAt = DateTime.UtcNow;

            _db.StockLedgers.Add(new StockLedger
            {
                BookId = book.BookId,
                ChangeType = StockChangeType.PurchaseIn,
                QtyChange = checked((int)item.Qty),
                QtyAfter = book.StockQty,
                Note = string.IsNullOrWhiteSpace(ReceiveInput.Note)
                    ? $"PO#{po.PurchaseOrderId} from {po.Supplier.SupplierName}"
                    : ReceiveInput.Note.Trim(),
                CreatedAt = DateTime.UtcNow
            });
        }

        po.Status = PurchaseOrderStatus.Received;
        po.ReceivedAt = DateTime.UtcNow;

        if (ReceiveInput.CloseOutOfStockRequests)
        {
            var requests = await _db.OutOfStockRequests
                .Where(r => r.BookId != null && bookIds.Contains(r.BookId.Value) && (r.Status == OutOfStockStatus.Submitted || r.Status == OutOfStockStatus.Processing || r.Status == OutOfStockStatus.Ordered))
                .ToListAsync();

            foreach (var r in requests)
            {
                r.Status = OutOfStockStatus.Completed;
                r.AdminReply = "已到货并入库";
                r.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToPage(new { id });
    }

    private async Task LoadAsync(long id)
    {
        var po = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (po is null)
        {
            Vm = null;
            return;
        }

        Vm = new PurchaseOrderVm
        {
            PurchaseOrderId = po.PurchaseOrderId,
            SupplierName = po.Supplier.SupplierName,
            Status = po.Status,
            CreatedAt = po.CreatedAt,
            ReceivedAt = po.ReceivedAt,
            Items = po.Items
                .OrderBy(i => i.PurchaseOrderItemId)
                .Select(i => new ItemVm
                {
                    PurchaseOrderItemId = i.PurchaseOrderItemId,
                    BookId = i.BookId,
                    BookNo = i.Book.BookNo,
                    VolumeNo = i.Book.VolumeNo,
                    Title = i.Book.Title,
                    Qty = i.Qty,
                    UnitCost = i.UnitCost,
                    StockQty = i.Book.StockQty
                })
                .ToList()
        };
    }
}
