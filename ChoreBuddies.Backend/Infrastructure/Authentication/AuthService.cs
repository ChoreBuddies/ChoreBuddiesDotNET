using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.Email;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Features.Users;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using Microsoft.AspNetCore.Identity;
using Shared.Authentication;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

public interface IAuthService
{
    public Task<AuthResponseDto> SignupUserAsync(RegisterRequestDto registerRequest);
    public Task<AuthResponseDto> LoginUserAsync(LoginRequestDto loginRequest);
    public Task<bool> RevokeRefreshTokenAsync(int userID);
    public Task<string> GenerateAccessTokenAsync(int userId);
}

public class AuthService(UserManager<AppUser> userManager, ITokenService tokenService, TimeProvider timeProvider, IEmailService emailService, INotificationPreferenceService notificationPreferenceService, IAppUserService userService) : IAuthService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IEmailService _emailService = emailService;
    private readonly INotificationPreferenceService _notificationPreferenceService = notificationPreferenceService;
    private readonly IAppUserService _userService = userService;
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

    public async Task<AuthResponseDto> SignupUserAsync(RegisterRequestDto registerRequest)
    {
        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;
        if (userExists)
        {
            throw new UserAlreadyExistsException(registerRequest.Email);
        }

        var newUser = new AppUser
        {
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            DateOfBirth = registerRequest.DateOfBirth,
            UserName = registerRequest.UserName
        };

        var result = await _userManager.CreateAsync(newUser, registerRequest.Password);
        if (!result.Succeeded)
        {
            throw new RegistrationFailedException(result.Errors.Select(err => err.Description));
        }

        var roleUpdateResult = await _userService.UpdateUserRoleAsync(newUser.Id, AuthConstants.RoleAdult);
        if (!roleUpdateResult)
        {
            throw new RegistrationFailedException(result.Errors.Select(err => err.Description));
        }

        await _notificationPreferenceService.CreateDefaultPreferencesAsync(newUser);

        var accessToken = await _tokenService.CreateAccessTokenAsync(newUser);
        var refreshToken = _tokenService.CreateRefreshToken();

        newUser.RefreshToken = refreshToken;
        newUser.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(newUser);

        await _emailService.SendRegisterConfirmationNotificationAsync(newUser, newUser.UserName);

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

    public async Task<string> GenerateAccessTokenAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new ArgumentException("Invalid user identifier.");
        }

        return await _tokenService.CreateAccessTokenAsync(user);
    }
}
