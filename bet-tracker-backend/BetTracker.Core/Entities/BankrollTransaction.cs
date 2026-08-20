using BetTracker.Core.Enums;

namespace BetTracker.Core.Entities;

public class BankrollTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public BankrollTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public Guid? RelatedBetId { get; set; }
    public Guid? RelatedSessionId { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime OccurredAt { get; set; }

    public User User { get; set; } = null!;
}
