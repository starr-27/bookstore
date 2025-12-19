using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.OutOfStock;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public RequestVm? Vm { get; private set; }

    public OutOfStockStatus[] StatusOptions { get; } =
    [
        OutOfStockStatus.Submitted,
        OutOfStockStatus.Processing,
        OutOfStockStatus.Ordered,
        OutOfStockStatus.Completed,
        OutOfStockStatus.Rejected
    ];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class RequestVm
    {
        public long OutOfStockRequestId { get; set; }
        public string CustomerEmail { get; set; } = default!;
        public string BookTitle { get; set; } = default!;
        public uint RequestedQty { get; set; }
        public string StatusText { get; set; } = default!;
        public string? Note { get; set; }
    }

    public class InputModel
    {
        public OutOfStockStatus Status { get; set; }

        [StringLength(500)]
        public string? AdminReply { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        await LoadAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(id);
            return Page();
        }

        var entity = await _db.OutOfStockRequests.FirstOrDefaultAsync(r => r.OutOfStockRequestId == id);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Status = Input.Status;
        entity.AdminReply = string.IsNullOrWhiteSpace(Input.AdminReply) ? null : Input.AdminReply.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/OutOfStock/Index");
    }

    private async Task LoadAsync(long id)
    {
        Vm = await _db.OutOfStockRequests
            .AsNoTracking()
            .Where(r => r.OutOfStockRequestId == id)
            .Select(r => new RequestVm
            {
                OutOfStockRequestId = r.OutOfStockRequestId,
                CustomerEmail = _db.Users.Where(u => u.Id == r.CustomerId).Select(u => u.Email).FirstOrDefault() ?? r.CustomerId,
                BookTitle = r.BookTitle,
                RequestedQty = r.RequestedQty,
                StatusText = r.Status.ToString(),
                Note = r.Note
            })
            .FirstOrDefaultAsync();

        if (Vm is null)
        {
            return;
        }

        var entity = await _db.OutOfStockRequests.AsNoTracking().FirstAsync(r => r.OutOfStockRequestId == id);
        Input.Status = entity.Status;
        Input.AdminReply = entity.AdminReply;
    }
}
