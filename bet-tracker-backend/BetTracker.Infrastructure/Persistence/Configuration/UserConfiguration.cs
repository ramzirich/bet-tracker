using BetTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BetTracker.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PreferredCurrency).IsRequired().HasMaxLength(3).HasDefaultValue("USD");

        builder.Property(u => u.InitialBankroll).HasPrecision(18, 2);
        builder.Property(u => u.CurrentBankroll).HasPrecision(18, 2);
        builder.Property(u => u.UnitSize).HasPrecision(18, 2).HasDefaultValue(10m);
        builder.Property(u => u.WeeklyLossLimit).HasPrecision(18, 2);
        builder.Property(u => u.DailyLossLimit).HasPrecision(18, 2);
    }
}
