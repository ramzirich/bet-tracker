using BetTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BetTracker.Infrastructure.Persistence;

public class BetTrackerDbContext: DbContext
{
    public BetTrackerDbContext(DbContextOptions<BetTrackerDbContext> options):
        base(options){}
    public DbSet<User> Users => Set<User>();
    public DbSet<BankrollTransaction> BankrollTransactions => Set<BankrollTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BetTrackerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}