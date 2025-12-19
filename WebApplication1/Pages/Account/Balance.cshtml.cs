using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Account;

[Authorize]
public class BalanceModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BalanceModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public CustomerProfile? Profile { get; private set; }

    [BindProperty]
    [Range(0.01, 99999999)]
    public decimal Amount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var profile = await _db.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile is null)
        {
            profile = new CustomerProfile
            {
                UserId = user.Id,
                FullName = string.Empty,
                Address = string.Empty,
                AccountBalance = 0m,
                CreditLevel = CreditLevel.Level1,
                OverdraftLimit = 0m
            };
            _db.CustomerProfiles.Add(profile);
        }

        profile.AccountBalance = decimal.Round(profile.AccountBalance + Amount, 2, MidpointRounding.AwayFromZero);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            Profile = null;
            return;
        }

        Profile = await _db.CustomerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id);
    }
}
