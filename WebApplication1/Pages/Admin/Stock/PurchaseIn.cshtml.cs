using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Stock;

public class PurchaseInModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public PurchaseInModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<SupplierOption> SupplierOptions { get; private set; } = new();
    public List<BookOption> BookOptions { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class SupplierOption
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = default!;
    }

    public class BookOption
    {
        public long BookId { get; set; }
        public string Title { get; set; } = default!;
    }

    public class InputModel
    {
        [Required]
        public long? SupplierId { get; set; }

        [Required]
        public long? BookId { get; set; }

        [Range(1, 1000000)]
        public uint Qty { get; set; } = 1;

        [Range(0.01, 999999.99)]
        public decimal UnitCost { get; set; } = 1;

        [StringLength(255)]
        public string? Note { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadOptionsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var supplier = await _db.Suppliers.FirstAsync(s => s.SupplierId == Input.SupplierId);
        var book = await _db.Books.FirstAsync(b => b.BookId == Input.BookId);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var po = new PurchaseOrder
        {
            SupplierId = supplier.SupplierId,
            Status = PurchaseOrderStatus.Received,
            CreatedAt = DateTime.UtcNow,
            ReceivedAt = DateTime.UtcNow,
            Items =
            [
                new PurchaseOrderItem
                {
                    BookId = book.BookId,
                    Qty = Input.Qty,
                    UnitCost = Input.UnitCost
                }
            ]
        };
        _db.PurchaseOrders.Add(po);

        var before = book.StockQty;
        book.StockQty = checked(book.StockQty + Input.Qty);

        _db.StockLedgers.Add(new StockLedger
        {
            BookId = book.BookId,
            ChangeType = StockChangeType.PurchaseIn,
            QtyChange = checked((int)Input.Qty),
            QtyAfter = book.StockQty,
            Note = string.IsNullOrWhiteSpace(Input.Note) ? $"Purchase from {supplier.SupplierName}" : Input.Note,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToPage("/Admin/Stock/Index", new { bookId = book.BookId });
    }

    private async Task LoadOptionsAsync()
    {
        SupplierOptions = await _db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.SupplierName)
            .Select(s => new SupplierOption { SupplierId = s.SupplierId, SupplierName = s.SupplierName })
            .ToListAsync();

        BookOptions = await _db.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .Select(b => new BookOption { BookId = b.BookId, Title = b.Title })
            .ToListAsync();
    }
}
