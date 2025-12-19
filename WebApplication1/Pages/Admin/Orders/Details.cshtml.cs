using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Orders;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public OrderVm? Order { get; private set; }

    [BindProperty]
    public ShipInputModel ShipInput { get; set; } = new();

    public class ShipInputModel
    {
        [Required, StringLength(50)]
        public string Carrier { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string TrackingNo { get; set; } = string.Empty;
    }

    public class OrderVm
    {
        public long OrderId { get; set; }
        public string CustomerId { get; set; } = default!;
        public string? CustomerEmail { get; set; }
        public string StatusText { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public string ReceiverName { get; set; } = default!;
        public string ReceiverPhone { get; set; } = default!;
        public string ReceiverAddr { get; set; } = default!;
        public string? Carrier { get; set; }
        public string? TrackingNo { get; set; }
        public List<ItemVm> Items { get; set; } = new();
    }

    public class ItemVm
    {
        public string Title { get; set; } = default!;
        public uint Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        await LoadAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostShipAsync(long id)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(id);
            return Page();
        }

        var order = await _db.Orders
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order is null)
        {
            return NotFound();
        }

        if (order.Shipment is null)
        {
            order.Shipment = new Shipment { OrderId = order.OrderId };
            _db.Shipments.Add(order.Shipment);
        }

        order.Shipment.Carrier = ShipInput.Carrier.Trim();
        order.Shipment.TrackingNo = ShipInput.TrackingNo.Trim();
        order.Shipment.ShippedAt = DateTime.UtcNow;

        order.OrderStatus = OrderStatus.Shipped;

        await _db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    private async Task LoadAsync(long id)
    {
        Order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Shipment)
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .Where(o => o.OrderId == id)
            .Select(o => new OrderVm
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                CustomerEmail = _db.Users.Where(u => u.Id == o.CustomerId).Select(u => u.Email).FirstOrDefault(),
                StatusText = o.OrderStatus.ToString(),
                TotalAmount = o.TotalAmount,
                ReceiverName = o.ReceiverName,
                ReceiverPhone = o.ReceiverPhone,
                ReceiverAddr = o.ReceiverAddr,
                Carrier = o.Shipment != null ? o.Shipment.Carrier : null,
                TrackingNo = o.Shipment != null ? o.Shipment.TrackingNo : null,
                Items = o.Items.Select(i => new ItemVm
                {
                    Title = i.Book.Title,
                    Qty = i.Qty,
                    UnitPrice = i.UnitPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (Order is not null)
        {
            ShipInput.Carrier = Order.Carrier ?? string.Empty;
            ShipInput.TrackingNo = Order.TrackingNo ?? string.Empty;
        }
    }
}
