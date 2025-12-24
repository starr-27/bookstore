using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Data.Entities;

namespace WebApplication1.Pages.Admin.Customers;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [TempData]
    public string? SuccessMessage { get; set; }

    public CustomerVm? Vm { get; private set; }

    public CreditLevel[] CreditLevels { get; } =
    [
        CreditLevel.Level1,
        CreditLevel.Level2,
        CreditLevel.Level3,
        CreditLevel.Level4,
        CreditLevel.Level5
    ];

    public string CreditRulesText { get; } = "L1=10% 不可透支；L2=15% 不可透支；L3=15% 可先发书后付款(限额)；L4=20% 可先发书后付款(限额)；L5=25% 可先发书后付款(不限额)";

    [BindProperty]
    public ProfileInputModel ProfileInput { get; set; } = new();

    [BindProperty]
    public RechargeInputModel RechargeInput { get; set; } = new();

    [BindProperty]
    public CreditInputModel CreditInput { get; set; } = new();

    public class CustomerVm
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public decimal AccountBalance { get; set; }
        public CreditLevel CreditLevel { get; set; }
        public decimal OverdraftLimit { get; set; }
    }

    public class ProfileInputModel
    {
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;
    }

    public class RechargeInputModel
    {
        [Range(0.01, 99999999)]
        public decimal Amount { get; set; }
    }

    public class CreditInputModel
    {
        public CreditLevel CreditLevel { get; set; } = CreditLevel.Level1;

        [Range(0, 99999999)]
        public decimal OverdraftLimit { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        await LoadAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveProfileAsync(string id)
    {
        // Ignore other form sections' fields on this post (credit/recharge)
        ModelState.Remove($"{nameof(CreditInput)}.{nameof(CreditInputModel.CreditLevel)}");
        ModelState.Remove($"{nameof(CreditInput)}.{nameof(CreditInputModel.OverdraftLimit)}");
        ModelState.Remove($"{nameof(RechargeInput)}.{nameof(RechargeInputModel.Amount)}");

        if (!TryValidateModel(ProfileInput, nameof(ProfileInput)))
        {
            await LoadAsync(id);
            return Page();
        }

        var profile = await GetOrCreateProfileAsync(id);

        profile.FullName = ProfileInput.FullName?.Trim() ?? string.Empty;
        profile.Address = ProfileInput.Address?.Trim() ?? string.Empty;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Customers/Edit", new { id });
    }

    public async Task<IActionResult> OnPostSaveAsync(string id)
    {
        await LoadAsync(id);
        if (Vm is null)
        {
            return NotFound();
        }

        await TryUpdateModelAsync(ProfileInput, nameof(ProfileInput));
        await TryUpdateModelAsync(CreditInput, nameof(CreditInput));

        var creditLevel = CreditInput.CreditLevel;
        var overdraftLimit = decimal.Round(CreditInput.OverdraftLimit, 2, MidpointRounding.AwayFromZero);

        if (!CreditLevelRules.CanOverdraft(creditLevel))
        {
            overdraftLimit = 0m;
        }
        else if (CreditLevelRules.UnlimitedOverdraft(creditLevel))
        {
            overdraftLimit = 0m;
        }
        else
        {
            if (overdraftLimit <= 0m)
            {
                ModelState.AddModelError($"{nameof(CreditInput)}.{nameof(CreditInputModel.OverdraftLimit)}", "L3/L4 必须设置大于 0 的透支额度");
                await LoadAsync(id);
                return Page();
            }
        }

        var profile = await GetOrCreateProfileAsync(id);
        profile.FullName = ProfileInput.FullName?.Trim() ?? string.Empty;
        profile.Address = ProfileInput.Address?.Trim() ?? string.Empty;
        profile.CreditLevel = creditLevel;
        profile.OverdraftLimit = overdraftLimit;

        if (!TryValidateModel(ProfileInput, nameof(ProfileInput)) || !TryValidateModel(CreditInput, nameof(CreditInput)))
        {
            await LoadAsync(id);
            return Page();
        }

        await _db.SaveChangesAsync();
        SuccessMessage = "信息已更新";
        return RedirectToPage("/Admin/Customers/Edit", new { id });
    }

    public async Task<IActionResult> OnPostRechargeAsync(string id)
    {
        // Ignore other form sections' fields on this post (profile/credit)
        ModelState.Remove($"{nameof(ProfileInput)}.{nameof(ProfileInputModel.FullName)}");
        ModelState.Remove($"{nameof(ProfileInput)}.{nameof(ProfileInputModel.Address)}");
        ModelState.Remove($"{nameof(CreditInput)}.{nameof(CreditInputModel.CreditLevel)}");
        ModelState.Remove($"{nameof(CreditInput)}.{nameof(CreditInputModel.OverdraftLimit)}");

        if (!ModelState.IsValid)
        {
            await LoadAsync(id);
            return Page();
        }

        var profile = await GetOrCreateProfileAsync(id);
        profile.AccountBalance = decimal.Round(profile.AccountBalance + RechargeInput.Amount, 2, MidpointRounding.AwayFromZero);

        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/Customers/Edit", new { id });
    }

    public async Task<IActionResult> OnPostUpdateCreditAsync(string id)
    {
        // Ignore other form sections' fields on this post (profile/recharge)
        ModelState.Remove($"{nameof(ProfileInput)}.{nameof(ProfileInputModel.FullName)}");
        ModelState.Remove($"{nameof(ProfileInput)}.{nameof(ProfileInputModel.Address)}");
        ModelState.Remove($"{nameof(RechargeInput)}.{nameof(RechargeInputModel.Amount)}");

        if (!ModelState.IsValid)
        {
            await LoadAsync(id);
            return Page();
        }

        var creditLevel = CreditInput.CreditLevel;
        var overdraftLimit = decimal.Round(CreditInput.OverdraftLimit, 2, MidpointRounding.AwayFromZero);

        if (!CreditLevelRules.CanOverdraft(creditLevel))
        {
            overdraftLimit = 0m;
        }
        else if (CreditLevelRules.UnlimitedOverdraft(creditLevel))
        {
            overdraftLimit = 0m;
        }
        else
        {
            if (overdraftLimit <= 0m)
            {
                ModelState.AddModelError($"{nameof(CreditInput)}.{nameof(CreditInputModel.OverdraftLimit)}", "L3/L4 必须设置大于 0 的透支额度");
                await LoadAsync(id);
                return Page();
            }
        }

        var profile = await GetOrCreateProfileAsync(id);
        profile.CreditLevel = creditLevel;
        profile.OverdraftLimit = overdraftLimit;

        await _db.SaveChangesAsync();
        SuccessMessage = "信用信息已更新";
        return RedirectToPage("/Admin/Customers/Edit", new { id });
    }

    private async Task<CustomerProfile> GetOrCreateProfileAsync(string userId)
    {
        var profile = await _db.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null)
        {
            profile = new CustomerProfile
            {
                UserId = userId,
                FullName = string.Empty,
                Address = string.Empty,
                AccountBalance = 0m,
                CreditLevel = CreditLevel.Level1,
                OverdraftLimit = 0m
            };
            _db.CustomerProfiles.Add(profile);
        }

        return profile;
    }

    private async Task LoadAsync(string userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.UserType == UserType.Customer);
        if (user is null)
        {
            Vm = null;
            return;
        }

        var profile = await _db.CustomerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        Vm = new CustomerVm
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? user.Id,
            AccountBalance = profile?.AccountBalance ?? 0m,
            CreditLevel = profile?.CreditLevel ?? CreditLevel.Level1,
            OverdraftLimit = profile?.OverdraftLimit ?? 0m
        };

        ProfileInput.FullName = profile?.FullName ?? string.Empty;
        ProfileInput.Address = profile?.Address ?? string.Empty;
        CreditInput.CreditLevel = profile?.CreditLevel ?? CreditLevel.Level1;
        CreditInput.OverdraftLimit = profile?.OverdraftLimit ?? 0m;
    }
}
