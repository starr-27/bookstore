using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Books;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<CategoryOption> CategoryOptions { get; private set; } = new();

    public class CategoryOption
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
    }

    public class InputModel
    {
        [Required, StringLength(32)]
        public string BookNo { get; set; } = string.Empty;

        [StringLength(40)]
        public string? VolumeNo { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; } = 1;

        [Range(0, 1000000)]
        public uint StockQty { get; set; } = 0;

        public long? CategoryId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadOptionsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var book = new Book
        {
            BookNo = Input.BookNo.Trim(),
            Title = Input.Title.Trim(),
            VolumeNo = string.IsNullOrWhiteSpace(Input.VolumeNo) ? null : Input.VolumeNo.Trim(),
            Price = Input.Price,
            StockQty = Input.StockQty,
            CategoryId = Input.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Books.Add(book);
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
