using BetTracker.Core.Dtos.Auth;

namespace BetTracker.Core.Abstractions;

public interface IAuthService
{
    Task<UserProfileDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
}
