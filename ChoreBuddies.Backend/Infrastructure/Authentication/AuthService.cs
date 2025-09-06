using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using ChoreBuddies_SharedModels.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

public interface IAuthService
{
    public Task RegisterUserAsync(RegisterRequestDto registerRequest);
    public Task<AuthResultDto> LoginUserAsync(LoginRequestDto loginRequest);
}

public class AuthService(UserManager<ApplicationUser> userManager) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<AuthResultDto> LoginUserAsync(LoginRequestDto loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        //var (jwtToken, expiresAt) = _tokenGenerator.GenerateToken(user);
        //var refreshToken = _tokenGenerator.GenerateRefreshToken();

        //var refreshTokenExpiration = DateTime.UtcNow.AddDays(7); // TIME PROVIDER and CONST

        //user.RefreshToken = refreshToken;
        //user.RefreshTokenExpiresAt = refreshTokenExpiration;

        //await _userManager.UpdateAsync(user);

        //return new AuthResultDto(
        //    jwtToken,
        //    expiresAt,
        //    refreshToken,
        //    refreshTokenExpiration
        //    );
        return new AuthResultDto("", DateTime.Now, "", DateTime.Now);
    }

    public async Task RegisterUserAsync(RegisterRequestDto registerRequest)
    {
        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;
        if (userExists)
        {
            throw new UserAlreadyExistsException(registerRequest.Email);
        }

        var newUser = new ApplicationUser
        {
            Email = registerRequest.Email,
            UserName = registerRequest.UserName
        };
        var result = await _userManager.CreateAsync(newUser, registerRequest.Password);

        if (!result.Succeeded)
        {
            throw new RegistrationFailedException(result.Errors.Select(err => err.Description));
        }
    }
}
