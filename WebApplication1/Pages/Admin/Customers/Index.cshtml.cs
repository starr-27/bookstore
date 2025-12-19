using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Customers;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string? Query { get; set; }
    public List<Row> Customers { get; private set; } = new();

    public class Row
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal AccountBalance { get; set; }
        public int CreditLevel { get; set; }
        public decimal OverdraftLimit { get; set; }
    }

    public async Task OnGetAsync(string? q)
    {
        Query = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var users = _db.Users.AsNoTracking().Where(u => u.UserType == UserType.Customer);
        if (!string.IsNullOrWhiteSpace(Query))
        {
            var like = $"%{Query}%";
            users = users.Where(u => EF.Functions.Like(u.Email!, like));
        }

        Customers = await users
            .OrderBy(u => u.Email)
            .Take(200)
            .GroupJoin(
                _db.CustomerProfiles.AsNoTracking(),
                u => u.Id,
                p => p.UserId,
                (u, ps) => new { u, profile = ps.FirstOrDefault() })
            .Select(x => new Row
            {
                UserId = x.u.Id,
                Email = x.u.Email!,
                FullName = x.profile != null ? x.profile.FullName : string.Empty,
                Address = x.profile != null ? x.profile.Address : string.Empty,
                AccountBalance = x.profile != null ? x.profile.AccountBalance : 0m,
                CreditLevel = x.profile != null ? (int)x.profile.CreditLevel : 1,
                OverdraftLimit = x.profile != null ? x.profile.OverdraftLimit : 0m
            })
            .ToListAsync();
    }
}
