namespace BetTracker.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public decimal InitialBankroll { get; set; }
    public decimal CurrentBankroll { get; set; }
    public string PreferredCurrency { get; set; } = "USD";
    public decimal UnitSize { get; set; } = 10m;
    public decimal? WeeklyLossLimit { get; set; }
    public decimal? DailyLossLimit { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<BankrollTransaction> BankrollTransactions { get; set; } = new List<BankrollTransaction>();
}
