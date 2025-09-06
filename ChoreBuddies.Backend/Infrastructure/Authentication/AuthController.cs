using ChoreBuddies_SharedModels.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

[ApiController]
[Route("/api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginUserAsync(request);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        await _authService.RegisterUserAsync(request);
        return Ok();
    }
}
