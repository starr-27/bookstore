using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Suppliers;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
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

        public bool IsActive { get; set; } = true;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var supplier = new Supplier
        {
            SupplierName = Input.SupplierName.Trim(),
            ContactName = string.IsNullOrWhiteSpace(Input.ContactName) ? null : Input.ContactName.Trim(),
            Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
            Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim(),
            IsActive = Input.IsActive
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Admin/Suppliers/Index");
    }
}
