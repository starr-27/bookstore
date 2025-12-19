using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Orders;

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

    public List<OrderListItem> Orders { get; private set; } = new();

    public class OrderListItem
    {
        public long OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusText { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public string Summary { get; set; } = default!;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            Orders = new();
            return;
        }

        Orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .Where(o => o.CustomerId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .Select(o => new OrderListItem
            {
                OrderId = o.OrderId,
                CreatedAt = o.CreatedAt,
                StatusText = o.OrderStatus.ToString(),
                TotalAmount = o.TotalAmount,
                Summary = string.Join("; ", o.Items.Select(i => $"{i.Book.Title} x{i.Qty}"))
            })
            .ToListAsync();
    }
}
