namespace BetTracker.Core.Dtos.Auth;

public record UserProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    decimal InitialBankroll,
    decimal CurrentBankroll,
    string PreferredCurrency,
    decimal UnitSize,
    decimal? DailyLossLimit,
    decimal? WeeklyLossLimit,
    DateTime CreatedAt);
