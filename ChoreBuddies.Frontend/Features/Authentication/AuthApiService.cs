using ChoreBuddies.Frontend.Utilities;
using Shared.Authentication;

namespace ChoreBuddies.Frontend.Features.Authentication;

public interface IAuthApiService
{
    Task<Result> LoginAsync(LoginRequestDto request);
    Task<Result> SignupAsync(RegisterRequestDto request);
    Task RevokeAsync();
}

public class AuthApiService : IAuthApiService
{
    private readonly HttpClientUtils _httpUtils;
    private readonly IAuthService _authService;

    public AuthApiService(HttpClientUtils httpUtils, IAuthService authService)
    {
        _httpUtils = httpUtils;
        _authService = authService;
    }

    public async Task<Result> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpUtils.TryRequestAsync(
            () => _httpUtils.PostAsync<LoginRequestDto, AuthResponseDto>(AuthConstants.ApiEndpointLogin, request)
        );

        if (response?.AccessToken is not null && response?.RefreshToken is not null)
        {
            await _authService.LoginAsync(response.AccessToken, response.RefreshToken);
            return Result.Success();
        }

        return Result.Fail("Login failed. Please check your credentials.");
    }

    public async Task<Result> SignupAsync(RegisterRequestDto request)
    {
        var response = await _httpUtils.TryRequestAsync(
             () => _httpUtils.PostAsync<RegisterRequestDto, AuthResponseDto>(AuthConstants.ApiEndpointSignup, request)
        );

        if (response?.AccessToken is not null && response?.RefreshToken is not null)
        {
            await _authService.LoginAsync(response.AccessToken, response.RefreshToken); // TODO when we add email confirmation, we might not want to log in the user right away
            return Result.Success();
        }

        return Result.Fail("Registration failed. Please try again.");
    }

    public async Task RevokeAsync()
    {
        await _httpUtils.TryRequestAsync<object>(async () =>
        {
            await _httpUtils.PostAsync(AuthConstants.ApiEndpointRevoke, new { }, authorized: true);
            return null;
        });

        await _authService.LogoutAsync();
    }
}
