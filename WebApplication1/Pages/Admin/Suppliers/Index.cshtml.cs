using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Pages.Admin.Suppliers;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<SupplierRow> Suppliers { get; private set; } = new();

    public class SupplierRow
    {
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = default!;
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public int BookCount { get; set; }
    }

    public async Task OnGetAsync()
    {
        Suppliers = await _db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.SupplierName)
            .Select(s => new SupplierRow
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email,
                IsActive = s.IsActive,
                BookCount = _db.Books.Count(b => b.SupplierId == s.SupplierId)
            })
            .ToListAsync();
    }
}
