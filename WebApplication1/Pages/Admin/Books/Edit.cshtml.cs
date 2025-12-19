using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

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

    public class InputModel
    {
        public long BookId { get; set; }

        [Required, StringLength(32)]
        public string BookNo { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Range(0, 1000000)]
        public uint StockQty { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var book = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == id);
        if (book is null)
        {
            return Page();
        }

        Input = new InputModel
        {
            BookId = book.BookId,
            BookNo = book.BookNo,
            Title = book.Title,
            Price = book.Price,
            StockQty = book.StockQty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
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
        book.Title = Input.Title.Trim();
        book.Price = Input.Price;
        book.StockQty = Input.StockQty;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Books/Index");
    }
}
