using BetTracker.Core.Abstractions;
using BetTracker.Core.Common;
using BetTracker.Core.Dtos.Auth;
using BetTracker.Core.Entities;
using BetTracker.Core.Enums;
using BetTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BetTracker.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly BetTrackerDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;

    public AuthService(BetTrackerDbContext db, IPasswordHasher passwordHasher,
        IClock clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(
            u=>u.Email==email, cancellationToken 
        );
        if (exists)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var now = _clock.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            InitialBankroll = request.InitialBankroll,
            CurrentBankroll = request.InitialBankroll,
            PreferredCurrency = "USD",
            UnitSize = 10m,
            CreatedAt = now
        };

        var openingEntry = new BankrollTransaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Type = BankrollTransactionType.InitialBalance,
            Amount = request.InitialBankroll,
            BalanceAfter = request.InitialBankroll,
            OccurredAt = now
        };

        _db.Users.Add(user);
        _db.BankrollTransactions.Add(openingEntry);
        await _db.SaveChangesAsync(cancellationToken);
        return ToProfile(user);
    }

     private static UserProfileDto ToProfile(User user) => new(
        user.Id,
        user.Email,
        user.DisplayName,
        user.InitialBankroll,
        user.CurrentBankroll,
        user.PreferredCurrency,
        user.UnitSize,
        user.DailyLossLimit,
        user.WeeklyLossLimit,
        user.CreatedAt);
}