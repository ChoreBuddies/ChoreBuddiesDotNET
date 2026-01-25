using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ChoreBuddies.Frontend.Features.Authentication;

public interface IAuthService
{
    public Task<string?> GetTokenAsync();
    public Task<string?> GetRefreshTokenAsync();
    public Task<bool> RefreshTokenAsync();
    public Task<IEnumerable<Claim>> GetClaimsAsync();
    public Task<string> GetClaimValueAsync(string claimType);
    public Task<bool> IsAuthenticatedAsync();
    public Task LoginAsync(string token, string refreshToken);
    public Task LogoutAsync();
    public Task UpdateAccessTokenAsync(string token);
    public Task<int> GetUserIdAsync();
    public Task<string> GetUserEmailAsync();
    public Task<string> GetUserNameAsync();
    public Task<string> GetFirstNameAsync();
    public Task<string> GetLastNameAsync();
    public Task<int> GetHouseholdIdAsync();
    public Task<string> GetUserRoleAsync();

    public Task<bool> IsChildAsync();
}

public class AuthService(
    IHttpClientFactory httpClientFactory,
    ILocalStorageService localStorage,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigationManager) : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILocalStorageService _localStorage = localStorage;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task LoginAsync(string token, string refreshToken)
    {
        await _localStorage.SetItemAsStringAsync(AuthFrontendConstants.AuthTokenKey, token);
        await _localStorage.SetItemAsStringAsync(AuthFrontendConstants.RefreshToken, refreshToken);

        if (_authStateProvider is JwtAuthStateProvider jwtAuthStateProvider)
        {
            jwtAuthStateProvider.NotifyUserAuthentication(token);
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(AuthFrontendConstants.AuthTokenKey);
        await _localStorage.RemoveItemAsync(AuthFrontendConstants.RefreshToken);

        if (_authStateProvider is JwtAuthStateProvider jwtAuthStateProvider)
        {
            jwtAuthStateProvider.NotifyUserLogout();
        }
    }

    public async Task UpdateAccessTokenAsync(string token)
    {
        await _localStorage.SetItemAsStringAsync(AuthFrontendConstants.AuthTokenKey, token);

        if (_authStateProvider is JwtAuthStateProvider jwtAuthStateProvider)
        {
            jwtAuthStateProvider.NotifyUserAuthentication(token);
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsStringAsync(AuthFrontendConstants.AuthTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _localStorage.GetItemAsStringAsync(AuthFrontendConstants.RefreshToken);
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var token = await GetTokenAsync();
        var refreshToken = await GetRefreshTokenAsync();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
        {
            await HandleLogOut();
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(AuthFrontendConstants.UnauthorizedClient);

            var response = await client.PostAsJsonAsync(AuthFrontendConstants.ApiEndpointRefresh,
                new RefreshTokenRequestDto(token, refreshToken));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (result != null)
                {
                    await LoginAsync(result.AccessToken, result.RefreshToken);
                    return true;
                }
            }
        }
        catch
        {
            // Token refresh failed
        }

        await HandleLogOut();
        return false;
    }

    public async Task<int> GetUserIdAsync() => Int32.TryParse(await GetClaimValueAsync(JwtRegisteredClaimNames.NameId), out var x) ? x : -1;

    public async Task<string> GetUserEmailAsync() => await GetClaimValueAsync(JwtRegisteredClaimNames.Email);

    public async Task<string> GetFirstNameAsync() => await GetClaimValueAsync(JwtRegisteredClaimNames.GivenName);

    public async Task<string> GetLastNameAsync() => await GetClaimValueAsync(JwtRegisteredClaimNames.FamilyName);

    public async Task<string> GetUserNameAsync() => await GetClaimValueAsync(JwtRegisteredClaimNames.UniqueName);

    public async Task<int> GetHouseholdIdAsync() => Int32.TryParse(await GetClaimValueAsync(AuthConstants.JwtHouseholdId), out var x) ? x : -1;

    public async Task<string> GetUserRoleAsync() => await GetClaimValueAsync(AuthConstants.JwtRole);

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Claims;
    }

    public async Task<string> GetClaimValueAsync(string claimType)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.FindFirst(claimType)?.Value ?? string.Empty;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Identity?.IsAuthenticated ?? false;
    }
    private async Task HandleLogOut()
    {
        await LogoutAsync();
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }

    public async Task<bool> IsChildAsync()
    {
        var role = await GetUserRoleAsync();
        if (string.IsNullOrEmpty(role)) return true;
        return String.Compare(role, AuthConstants.RoleChild, true) == 0;
    }
}

