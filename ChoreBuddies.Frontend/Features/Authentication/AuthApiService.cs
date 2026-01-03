using ChoreBuddies.Frontend.Utilities;
using Shared.Authentication;

namespace ChoreBuddies.Frontend.Features.Authentication;

public interface IAuthApiService
{
    Task<bool> LoginAsync(LoginRequestDto request);
    Task<bool> SignupAsync(RegisterRequestDto request);
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

    public async Task<bool> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpUtils.PostAsync<LoginRequestDto, AuthResponseDto>(
            AuthFrontendConstants.ApiEndpointLogin, request);

        if (response?.AccessToken is not null && response?.RefreshToken is not null)
        {
            await _authService.LoginAsync(response.AccessToken, response.RefreshToken);
            return true;
        }

        return false;
    }

    public async Task<bool> SignupAsync(RegisterRequestDto request)
    {
        var response = await _httpUtils.PostAsync<RegisterRequestDto, AuthResponseDto>(
            AuthFrontendConstants.ApiEndpointSignup, request);

        if (response?.AccessToken is not null && response?.RefreshToken is not null)
        {
            await _authService.LoginAsync(response.AccessToken, response.RefreshToken);
            return true;
        }

        return false;
    }

    public async Task RevokeAsync()
    {
        await _httpUtils.PostAsync(AuthFrontendConstants.ApiEndpointRevoke, new { }, authorized: true);
        await _authService.LogoutAsync();
    }
}
