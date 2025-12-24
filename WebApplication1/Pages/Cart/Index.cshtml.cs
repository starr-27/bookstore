using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;
using WebApplication1.Services;

namespace WebApplication1.Pages.Cart;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICartService _cart;
    private readonly IAssetManifest _assets;
    private readonly IVisualSelector _visuals;

    public IndexModel(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ICartService cart,
        IAssetManifest assets,
        IVisualSelector visuals)
    {
        _db = db;
        _userManager = userManager;
        _cart = cart;
        _assets = assets;
        _visuals = visuals;
    }

    public List<CartItemVm> Items { get; private set; } = new();
    public decimal TotalAmount { get; private set; }

    [BindProperty]
    public CheckoutInputModel CheckoutInput { get; set; } = new();

    public class CheckoutInputModel
    {
        [Required, StringLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string ReceiverPhone { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string ReceiverAddr { get; set; } = string.Empty;
    }

    public class CartItemVm
    {
        public long BookId { get; set; }
        public string BookNo { get; set; } = default!;
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public uint StockQty { get; set; }
        public uint Qty { get; set; }
        public string CoverUrl { get; set; } = "/images/book-cover-1.svg";
        public decimal LineAmount => Price * Qty;
    }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSetQtyAsync(long bookId, uint qty)
    {
        _cart.Set(bookId, qty);
        TempData["SuccessMessage"] = "购物车已更新";
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(long bookId)
    {
        _cart.Remove(bookId);
        TempData["SuccessMessage"] = "已移除";
        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        _cart.Clear();
        TempData["SuccessMessage"] = "购物车已清空";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        await LoadAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Items.Count == 0)
        {
            TempData["WarningMessage"] = "购物车为空";
            return RedirectToPage();
        }

        // Validate quantities
        foreach (var i in Items)
        {
            if (i.Qty == 0) continue;
            if (i.Qty > i.StockQty)
            {
                ModelState.AddModelError(string.Empty, $"《{i.Title}》库存不足（库存 {i.StockQty}）");
                return Page();
            }
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            CustomerId = user.Id,
            OrderStatus = OrderStatus.Created,
            ReceiverName = CheckoutInput.ReceiverName.Trim(),
            ReceiverPhone = CheckoutInput.ReceiverPhone.Trim(),
            ReceiverAddr = CheckoutInput.ReceiverAddr.Trim(),
            CreatedAt = DateTime.UtcNow,
            PaidAt = null,
        };

        foreach (var i in Items.Where(x => x.Qty > 0))
        {
            order.Items.Add(new OrderItem
            {
                BookId = i.BookId,
                Qty = i.Qty,
                UnitPrice = i.Price
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.UnitPrice * x.Qty);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Reduce stock
        var bookIds = order.Items.Select(x => x.BookId).ToArray();
        var books = await _db.Books.Where(b => bookIds.Contains(b.BookId)).ToListAsync();
        foreach (var line in order.Items)
        {
            var b = books.First(x => x.BookId == line.BookId);
            b.StockQty = checked(b.StockQty - line.Qty);
            b.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _cart.Clear();
        TempData["SuccessMessage"] = "订单已创建，去支付吧！";
        return RedirectToPage("/Orders/Details", new { id = order.OrderId });
    }

    private async Task LoadAsync()
    {
        var cart = _cart.GetItems();
        if (cart.Count == 0)
        {
            Items = new();
            TotalAmount = 0m;
            return;
        }

        var ids = cart.Keys.ToArray();
        var books = await _db.Books
            .AsNoTracking()
            .Where(b => ids.Contains(b.BookId))
            .Select(b => new
            {
                b.BookId,
                b.BookNo,
                b.Title,
                b.Price,
                b.StockQty
            })
            .ToListAsync();

        Items = books
            .Select(b => new CartItemVm
            {
                BookId = b.BookId,
                BookNo = b.BookNo,
                Title = b.Title,
                Price = b.Price,
                StockQty = b.StockQty,
                Qty = cart.TryGetValue(b.BookId, out var q) ? q : 0,
                CoverUrl = _visuals.PickStableByLong(_assets.GetFlowerCovers(), b.BookId)
                    ?? $"/images/book-cover-{((b.BookId % 3) + 1)}.svg"
            })
            .OrderBy(x => x.Title)
            .ToList();

        TotalAmount = Items.Sum(x => x.LineAmount);
    }
}
