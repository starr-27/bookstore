using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin.Stock;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public long? BookId { get; set; }

    public List<BookOption> BookOptions { get; private set; } = new();
    public List<Row> Rows { get; private set; } = new();

    public class BookOption
    {
        public long BookId { get; set; }
        public string Title { get; set; } = default!;
    }

    public class Row
    {
        public DateTime CreatedAt { get; set; }
        public string BookTitle { get; set; } = default!;
        public string ChangeType { get; set; } = default!;
        public int QtyChange { get; set; }
        public uint QtyAfter { get; set; }
        public string? Note { get; set; }
    }

    public async Task OnGetAsync(long? bookId)
    {
        BookId = bookId;

        BookOptions = await _db.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .Take(500)
            .Select(b => new BookOption { BookId = b.BookId, Title = b.Title })
            .ToListAsync();

        var query = _db.StockLedgers
            .AsNoTracking()
            .Include(x => x.Book)
            .AsQueryable();

        if (BookId.HasValue)
        {
            query = query.Where(x => x.BookId == BookId.Value);
        }

        Rows = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new Row
            {
                CreatedAt = x.CreatedAt,
                BookTitle = x.Book.Title,
                ChangeType = x.ChangeType.ToString(),
                QtyChange = x.QtyChange,
                QtyAfter = x.QtyAfter,
                Note = x.Note
            })
            .ToListAsync();
    }
}
