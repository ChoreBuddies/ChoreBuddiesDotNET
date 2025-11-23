using ChoreBuddies.Frontend.Features.Authentication;
using Shared.Users;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Features.User;

public interface IUserService
{
    public Task<AppUserDto?> GetCurrentUserAsync();
    public Task<bool> UpdateUserAsync(AppUserDto user);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(AuthConstants.AuthorizedClient);
    }

    public async Task<AppUserDto?> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync(UserConstants.ApiEndpointMe);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch user. Status: {response.StatusCode}");

            return null;
        }

        return await response.Content.ReadFromJsonAsync<AppUserDto>();
    }

    public async Task<bool> UpdateUserAsync(AppUserDto user)
    {
        var response = await _httpClient.PutAsJsonAsync(UserConstants.ApiEndpointMe, user);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to update user. Status: {response.StatusCode}");
            return false;
        }

        return true;
    }
}
