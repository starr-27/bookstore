using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin.Books;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string? Query { get; set; }
    public List<BookRow> Books { get; private set; } = new();

    public class BookRow
    {
        public long BookId { get; set; }
        public string BookNo { get; set; } = default!;
        public string? VolumeNo { get; set; }
        public string Title { get; set; } = default!;
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public uint StockQty { get; set; }
    }

    public async Task OnGetAsync(string? q)
    {
        Query = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var query = _db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(Query))
        {
            var like = $"%{Query}%";
            query = query.Where(b =>
                EF.Functions.Like(b.Title, like) ||
                EF.Functions.Like(b.BookNo, like) ||
                (b.VolumeNo != null && EF.Functions.Like(b.VolumeNo, like)) ||
                (b.Category != null && EF.Functions.Like(b.Category.CategoryName, like)));
        }

        Books = await query
            .OrderByDescending(b => b.UpdatedAt)
            .Take(200)
            .Select(b => new BookRow
            {
                BookId = b.BookId,
                BookNo = b.BookNo,
                VolumeNo = b.VolumeNo,
                Title = b.Title,
                CategoryName = b.Category != null ? b.Category.CategoryName : null,
                Price = b.Price,
                StockQty = b.StockQty
            })
            .ToListAsync();
    }
}
