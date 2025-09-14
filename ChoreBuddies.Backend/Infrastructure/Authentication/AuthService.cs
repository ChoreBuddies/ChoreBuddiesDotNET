using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using Microsoft.AspNetCore.Identity;
using Shared.Authentication;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto registerRequest);
    public Task<AuthResponseDto> LoginUserAsync(LoginRequestDto loginRequest);
    public Task<bool> RevokeRefreshTokenAsync(int userID);
}

public class AuthService(UserManager<AppUser> userManager, ITokenService tokenService, TimeProvider timeProvider) : IAuthService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<AuthResponseDto> LoginUserAsync(LoginRequestDto loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        var accessToken = await _tokenService.CreateAccessTokenAsync(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto(accessToken, refreshToken);
    }

    public async Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto registerRequest)
    {
        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;
        if (userExists)
        {
            throw new UserAlreadyExistsException(registerRequest.Email);
        }

        var newUser = new AppUser
        {
            Email = registerRequest.Email,
            UserName = registerRequest.UserName
        };
        var result = await _userManager.CreateAsync(newUser, registerRequest.Password);

        if (!result.Succeeded)
        {
            throw new RegistrationFailedException(result.Errors.Select(err => err.Description));
        }

        var accessToken = await _tokenService.CreateAccessTokenAsync(newUser);
        var refreshToken = _tokenService.CreateRefreshToken();

        newUser.RefreshToken = refreshToken;
        newUser.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(newUser);

        return new AuthResponseDto(accessToken, refreshToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(int userID)
    {
        var user = await _userManager.FindByIdAsync(userID.ToString());
        if (user == null) return false;

        // Revoke the refresh token by setting it to null and expiring it
        user.RefreshToken = null;
        user.RefreshTokenExpiry = _timeProvider.GetUtcNow().DateTime;
        await _userManager.UpdateAsync(user);

        return true;
    }
}
