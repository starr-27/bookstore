using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.OutOfStock;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<BookOption> BookOptions { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class BookOption
    {
        public long BookId { get; set; }
        public string Title { get; set; } = default!;
    }

    public class InputModel
    {
        public long? BookId { get; set; }

        [StringLength(200)]
        public string? BookTitle { get; set; }

        [Range(1, 999999)]
        public uint RequestedQty { get; set; } = 1;

        [StringLength(500)]
        public string? Note { get; set; }
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

        if (Input.BookId is null && string.IsNullOrWhiteSpace(Input.BookTitle))
        {
            ModelState.AddModelError(nameof(Input.BookTitle), "请选择书籍或填写书名");
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        string title;
        if (Input.BookId is not null)
        {
            var book = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == Input.BookId);
            if (book is null)
            {
                ModelState.AddModelError(nameof(Input.BookId), "未找到该书籍");
                return Page();
            }

            title = book.Title;
        }
        else
        {
            title = Input.BookTitle!.Trim();
        }

        _db.OutOfStockRequests.Add(new OutOfStockRequest
        {
            CustomerId = user.Id,
            BookId = Input.BookId,
            BookTitle = title,
            RequestedQty = Input.RequestedQty,
            Note = string.IsNullOrWhiteSpace(Input.Note) ? null : Input.Note.Trim(),
            Status = OutOfStockStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return RedirectToPage("/OutOfStock/My");
    }

    private async Task LoadOptionsAsync()
    {
        BookOptions = await _db.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .Take(500)
            .Select(b => new BookOption { BookId = b.BookId, Title = b.Title })
            .ToListAsync();
    }
}
