using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Books;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public BookVm? Book { get; private set; }

    [BindProperty]
    [Range(1, 999)]
    public uint Qty { get; set; } = 1;

    [BindProperty, Required, StringLength(100)]
    public string ReceiverName { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(30)]
    public string ReceiverPhone { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(255)]
    public string ReceiverAddr { get; set; } = string.Empty;

    public class BookVm
    {
        public long BookId { get; set; }
        public string BookNo { get; set; } = default!;
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public uint StockQty { get; set; }
        public string? PublisherName { get; set; }
        public string? SupplierName { get; set; }
        public string AuthorsText { get; set; } = "-";
        public string KeywordsText { get; set; } = "-";
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        Book = await _db.Books
            .AsNoTracking()
            .Include(b => b.Publisher)
            .Include(b => b.Supplier)
            .Include(b => b.Authors).ThenInclude(ba => ba.Author)
            .Include(b => b.Keywords).ThenInclude(bk => bk.Keyword)
            .Where(b => b.BookId == id)
            .Select(b => new BookVm
            {
                BookId = b.BookId,
                BookNo = b.BookNo,
                Title = b.Title,
                Price = b.Price,
                StockQty = b.StockQty,
                PublisherName = b.Publisher != null ? b.Publisher.PublisherName : null,
                SupplierName = b.Supplier != null ? b.Supplier.SupplierName : null,
                AuthorsText = string.Join(", ", b.Authors.OrderBy(x => x.AuthorOrder).Select(x => x.Author.AuthorName)),
                KeywordsText = b.Keywords.Count == 0 ? "-" : string.Join(", ", b.Keywords.Select(x => x.Keyword.KeywordText))
            })
            .FirstOrDefaultAsync();

        return Page();
    }

    [Authorize]
    public async Task<IActionResult> OnPostBuyAsync(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return await OnGetAsync(id);
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book is null)
        {
            return NotFound();
        }

        if (book.StockQty < Qty)
        {
            _db.OutOfStockRequests.Add(new OutOfStockRequest
            {
                CustomerId = user.Id,
                BookId = book.BookId,
                BookTitle = book.Title,
                RequestedQty = Qty,
                Note = "下单数量超出库存，已自动登记缺书记录",
                Status = OutOfStockStatus.Submitted,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            ModelState.AddModelError(string.Empty, "库存不足，已自动登记缺书记录，管理员会尽快处理。你也可以在“缺书登记”查看处理进度。");
            return await OnGetAsync(id);
        }

        var profile = await _db.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile is null)
        {
            profile = new CustomerProfile
            {
                UserId = user.Id,
                FullName = string.Empty,
                Address = string.Empty,
                AccountBalance = 0m,
                CreditLevel = CreditLevel.Level1,
                OverdraftLimit = 0m
            };
            _db.CustomerProfiles.Add(profile);
        }

        var baseTotal = book.Price * Qty;
        var discountRate = CreditLevelRules.DiscountRate(profile.CreditLevel);
        var payable = decimal.Round(baseTotal * (1 - discountRate), 2, MidpointRounding.AwayFromZero);

        var canOverdraft = CreditLevelRules.CanOverdraft(profile.CreditLevel);
        var unlimited = CreditLevelRules.UnlimitedOverdraft(profile.CreditLevel);
        var available = profile.AccountBalance;
        var allowedNegative = unlimited ? decimal.MaxValue : profile.OverdraftLimit;

        if (!canOverdraft)
        {
            if (available < payable)
            {
                ModelState.AddModelError(string.Empty, $"余额不足，需支付 ?{payable:0.00}。当前余额 ?{available:0.00}。信用等级{(int)profile.CreditLevel}不可透支。请联系管理员充值。");
                return await OnGetAsync(id);
            }
        }
        else
        {
            // overdraft allowed: balance can go negative but must not exceed limit
            // balance after payment = available - payable >= -allowedNegative
            if (!unlimited && (available - payable) < -allowedNegative)
            {
                ModelState.AddModelError(string.Empty, $"超出信用透支额度，需支付 ?{payable:0.00}。当前余额 ?{available:0.00}，透支额度 ?{profile.OverdraftLimit:0.00}。请联系管理员调整额度或充值。");
                return await OnGetAsync(id);
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        // payment: deduct from balance (may become negative)
        profile.AccountBalance = decimal.Round(profile.AccountBalance - payable, 2, MidpointRounding.AwayFromZero);

        // stock out
        book.StockQty -= Qty;
        _db.StockLedgers.Add(new StockLedger
        {
            BookId = book.BookId,
            ChangeType = StockChangeType.SaleOut,
            QtyChange = -checked((int)Qty),
            QtyAfter = book.StockQty,
            Note = $"Sale to {user.Email}, credit L{(int)profile.CreditLevel}, discount {(discountRate * 100):0}%",
            CreatedAt = DateTime.UtcNow
        });

        var order = new Order
        {
            CustomerId = user.Id,
            OrderStatus = OrderStatus.Paid,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
            ReceiverName = ReceiverName,
            ReceiverPhone = ReceiverPhone,
            ReceiverAddr = ReceiverAddr,
            TotalAmount = payable,
            Items =
            [
                new OrderItem
                {
                    BookId = book.BookId,
                    Qty = Qty,
                    UnitPrice = book.Price
                }
            ]
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToPage("/Orders/Details", new { id = order.OrderId });
    }
}
