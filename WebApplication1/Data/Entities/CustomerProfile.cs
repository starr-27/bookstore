namespace WebApplication1.Data.Entities;

public enum CreditLevel
{
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5
}

public static class CreditLevelRules
{
    public static decimal DiscountRate(CreditLevel level) => level switch
    {
        CreditLevel.Level1 => 0.10m,
        CreditLevel.Level2 => 0.15m,
        CreditLevel.Level3 => 0.15m,
        CreditLevel.Level4 => 0.20m,
        CreditLevel.Level5 => 0.25m,
        _ => 0m,
    };

    public static bool CanOverdraft(CreditLevel level) => level >= CreditLevel.Level3;

    public static bool UnlimitedOverdraft(CreditLevel level) => level == CreditLevel.Level5;
}

public class CustomerProfile
{
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public decimal AccountBalance { get; set; } = 0m;

    public CreditLevel CreditLevel { get; set; } = CreditLevel.Level1;

    /// <summary>
    /// When CreditLevel allows overdraft, this is the maximum allowed negative balance.
    /// For unlimited overdraft (Level5), this value is ignored.
    /// </summary>
    public decimal OverdraftLimit { get; set; } = 0m;
}
