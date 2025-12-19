using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin.Suppliers;

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
        public long SupplierId { get; set; }

        [Required, StringLength(120)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(60)]
        public string? ContactName { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public bool IsActive { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.SupplierId == id);
        if (supplier is null)
        {
            return Page();
        }

        Input = new InputModel
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            ContactName = supplier.ContactName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id)
    {
        if (!ModelState.IsValid || Input is null)
        {
            return Page();
        }

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id);
        if (supplier is null)
        {
            return NotFound();
        }

        supplier.SupplierName = Input.SupplierName.Trim();
        supplier.ContactName = string.IsNullOrWhiteSpace(Input.ContactName) ? null : Input.ContactName.Trim();
        supplier.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();
        supplier.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
        supplier.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();
        supplier.IsActive = Input.IsActive;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Suppliers/Index");
    }
}
