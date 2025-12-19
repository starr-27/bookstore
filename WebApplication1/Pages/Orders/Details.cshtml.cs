using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Orders;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public OrderVm? Order { get; private set; }

    public class OrderVm
    {
        public long OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string StatusText { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public string ReceiverName { get; set; } = default!;
        public string ReceiverPhone { get; set; } = default!;
        public string ReceiverAddr { get; set; } = default!;
        public List<OrderItemVm> Items { get; set; } = new();
    }

    public class OrderItemVm
    {
        public string Title { get; set; } = default!;
        public uint Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        Order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .Where(o => o.OrderId == id && o.CustomerId == user.Id)
            .Select(o => new OrderVm
            {
                OrderId = o.OrderId,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                StatusText = o.OrderStatus.ToString(),
                TotalAmount = o.TotalAmount,
                ReceiverName = o.ReceiverName,
                ReceiverPhone = o.ReceiverPhone,
                ReceiverAddr = o.ReceiverAddr,
                Items = o.Items.Select(i => new OrderItemVm
                {
                    Title = i.Book.Title,
                    Qty = i.Qty,
                    UnitPrice = i.UnitPrice,
                    LineAmount = i.UnitPrice * i.Qty
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (Order is null)
        {
            return NotFound();
        }

        return Page();
    }
}
