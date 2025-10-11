using Docker.DotNet.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared.Authentication;
using System.Security.Claims;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

[ApiController]
[Route("/api/v1/[controller]")]
public class AuthController(IAuthService authService, ITokenService tokenService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginUserAsync(request);
        return Ok(result);
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.SignupUserAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _tokenService.RefreshAccessToken(request.AccessToken, request.RefreshToken);
            return Ok(response);
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("revoke")]
    [Authorize] // User must be authenticated to revoke their own token
    public async Task<IActionResult> Revoke()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || !int.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        if (await _authService.RevokeRefreshTokenAsync(id))
        {
            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }
}
