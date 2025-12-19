using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.OutOfStock;

[Authorize]
public class MyModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MyModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<Row> Rows { get; private set; } = new();

    public class Row
    {
        public DateTime CreatedAt { get; set; }
        public string BookTitle { get; set; } = default!;
        public uint RequestedQty { get; set; }
        public string StatusText { get; set; } = default!;
        public string? AdminReply { get; set; }
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            Rows = new();
            return;
        }

        Rows = await _db.OutOfStockRequests
            .AsNoTracking()
            .Where(r => r.CustomerId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(200)
            .Select(r => new Row
            {
                CreatedAt = r.CreatedAt,
                BookTitle = r.BookTitle,
                RequestedQty = r.RequestedQty,
                StatusText = r.Status.ToString(),
                AdminReply = r.AdminReply
            })
            .ToListAsync();
    }
}
