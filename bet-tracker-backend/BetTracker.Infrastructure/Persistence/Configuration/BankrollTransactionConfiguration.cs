using BetTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BetTracker.Infrastructure.Persistence.Configurations;

public class BankrollTransactionConfiguration : IEntityTypeConfiguration<BankrollTransaction>
{
    public void Configure(EntityTypeBuilder<BankrollTransaction> builder)
    {
        builder.ToTable("BankrollTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.BalanceAfter).HasPrecision(18, 2);
        builder.Property(t => t.Type).HasConversion<int>();

        builder.HasOne(t => t.User)
            .WithMany(u => u.BankrollTransactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.UserId, t.OccurredAt });
    }
}
