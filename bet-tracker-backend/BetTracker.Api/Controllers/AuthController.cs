using BetTracker.Core.Abstractions;
using BetTracker.Core.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BetTracker.Api.Controllers;
[ApiController]
[Route("api/auth")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserProfileDto),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
     public async Task<ActionResult<UserProfileDto>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var profile = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(profile);
    }

}