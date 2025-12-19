using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public string? Query { get; set; }

        public List<BookListItem> Books { get; private set; } = new();

        public class BookListItem
        {
            public long BookId { get; set; }
            public string BookNo { get; set; } = default!;
            public string Title { get; set; } = default!;
            public decimal Price { get; set; }
            public uint StockQty { get; set; }
            public string? PublisherName { get; set; }
            public string AuthorsText { get; set; } = "-";
        }

        public async Task OnGetAsync(string? q)
        {
            Query = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            var query = _db.Books
                .AsNoTracking()
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
                    b.Authors.Any(a => EF.Functions.Like(a.Author.AuthorName, like)) ||
                    b.Keywords.Any(k => EF.Functions.Like(k.Keyword.KeywordText, like)));
            }

            Books = await query
                .OrderBy(b => b.Title)
                .Take(60)
                .Select(b => new BookListItem
                {
                    BookId = b.BookId,
                    BookNo = b.BookNo,
                    Title = b.Title,
                    Price = b.Price,
                    StockQty = b.StockQty,
                    PublisherName = b.Publisher != null ? b.Publisher.PublisherName : null,
                    AuthorsText = string.Join(", ", b.Authors.OrderBy(x => x.AuthorOrder).Select(x => x.Author.AuthorName))
                })
                .ToListAsync();
        }
    }
}
