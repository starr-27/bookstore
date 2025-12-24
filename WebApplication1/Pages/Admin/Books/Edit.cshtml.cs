using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Books;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel? Input { get; set; }

    public List<CategoryOption> CategoryOptions { get; private set; } = new();

    public class CategoryOption
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
    }

    public class InputModel
    {
        public long BookId { get; set; }

        [Required, StringLength(32)]
        public string BookNo { get; set; } = string.Empty;

        [StringLength(40)]
        public string? VolumeNo { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Range(0, 1000000)]
        public uint StockQty { get; set; }

        public long? CategoryId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        await LoadOptionsAsync();

        var book = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == id);
        if (book is null)
        {
            return Page();
        }

        Input = new InputModel
        {
            BookId = book.BookId,
            BookNo = book.BookNo,
            VolumeNo = book.VolumeNo,
            Title = book.Title,
            Price = book.Price,
            StockQty = book.StockQty,
            CategoryId = book.CategoryId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        await LoadOptionsAsync();

        if (!ModelState.IsValid || Input is null)
        {
            return Page();
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book is null)
        {
            return NotFound();
        }

        book.BookNo = Input.BookNo.Trim();
        book.VolumeNo = string.IsNullOrWhiteSpace(Input.VolumeNo) ? null : Input.VolumeNo.Trim();
        book.Title = Input.Title.Trim();
        book.Price = Input.Price;
        book.StockQty = Input.StockQty;
        book.CategoryId = Input.CategoryId;
        book.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Books/Index");
    }

    private async Task LoadOptionsAsync()
    {
        CategoryOptions = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryOption { CategoryId = c.CategoryId, CategoryName = c.CategoryName })
            .ToListAsync();
    }
}
