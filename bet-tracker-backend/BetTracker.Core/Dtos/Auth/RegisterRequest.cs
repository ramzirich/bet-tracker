namespace BetTracker.Core.Dtos.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    decimal InitialBankroll);
