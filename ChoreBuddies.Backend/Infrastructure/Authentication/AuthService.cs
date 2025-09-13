using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using Microsoft.AspNetCore.Identity;
using Shared.Authentication;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto registerRequest);
    public Task<AuthResponseDto> LoginUserAsync(LoginRequestDto loginRequest);
}

public class AuthService(UserManager<AppUser> userManager, ITokenService tokenService) : IAuthService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<AuthResponseDto> LoginUserAsync(LoginRequestDto loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        var accessToken = await _tokenService.CreateAccessTokenAsync(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        // TODO: Save the refreshToken to the database associated with the user
        // user.RefreshToken = refreshToken;
        // user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
        // await _userManager.UpdateAsync(user);

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

        // TODO: Save the refreshToken to the database associated with the user
        // user.RefreshToken = refreshToken;
        // user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
        // await _userManager.UpdateAsync(user);

        return new AuthResponseDto(accessToken, refreshToken);
    }
}
