using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cart;

        public IndexModel(ApplicationDbContext db, ICartService cart)
        {
            _db = db;
            _cart = cart;
        }

        public string? Query { get; set; }

        public List<BookListItem> Books { get; private set; } = new();

        public class BookListItem
        {
            public long BookId { get; set; }
            public string BookNo { get; set; } = default!;
            public string? VolumeNo { get; set; }
            public string Title { get; set; } = default!;
            public decimal Price { get; set; }
            public uint StockQty { get; set; }
            public string? PublisherName { get; set; }
            public string? CategoryName { get; set; }
            public string AuthorsText { get; set; } = "-";
        }

        public async Task OnGetAsync(string? q)
        {
            Query = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            var query = _db.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.Authors).ThenInclude(ba => ba.Author)
                .Include(b => b.Keywords).ThenInclude(bk => bk.Keyword)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Query))
            {
                var like = $"%{Query}%";
                query = query.Where(b =>
                    EF.Functions.Like(b.Title, like) ||
                    EF.Functions.Like(b.BookNo, like) ||
                    (b.VolumeNo != null && EF.Functions.Like(b.VolumeNo, like)) ||
                    b.Authors.Any(a => EF.Functions.Like(a.Author.AuthorName, like)) ||
                    b.Keywords.Any(k => EF.Functions.Like(k.Keyword.KeywordText, like)) ||
                    (b.Category != null && EF.Functions.Like(b.Category.CategoryName, like)));
            }

            Books = await query
                .OrderBy(b => b.Title)
                .Take(60)
                .Select(b => new BookListItem
                {
                    BookId = b.BookId,
                    BookNo = b.BookNo,
                    VolumeNo = b.VolumeNo,
                    Title = b.Title,
                    Price = b.Price,
                    StockQty = b.StockQty,
                    PublisherName = b.Publisher != null ? b.Publisher.PublisherName : null,
                    CategoryName = b.Category != null ? b.Category.CategoryName : null,
                    AuthorsText = b.Authors.Count == 0
                        ? "-"
                        : string.Join(", ", b.Authors.OrderBy(x => x.AuthorOrder).Select(x => x.Author.AuthorName))
                })
                .ToListAsync();
        }

        public IActionResult OnPostAddToCart(long bookId, uint qty)
        {
            if (bookId <= 0 || qty == 0)
            {
                TempData["WarningMessage"] = "请选择有效的商品数量";
                return RedirectToPage(new { q = Query });
            }

            _cart.Add(bookId, qty);
            TempData["SuccessMessage"] = $"已加入购物车 ×{qty}";
            return RedirectToPage(new { q = Query });
        }
    }
}
