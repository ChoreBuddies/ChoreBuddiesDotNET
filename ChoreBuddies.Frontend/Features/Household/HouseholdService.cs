using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Frontend.Features.Authentication;
using Shared.Authentication;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Features.Household;

public interface IHouseholdService
{
    public Task<bool> JoinHouseholdAsync(string invitationCode);
}

public class HouseholdService : IHouseholdService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public HouseholdService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient(AuthFrontendConstants.AuthorizedClient);
        _authService = authService;
    }

    public async Task<bool> JoinHouseholdAsync(string invitationCode)
    {
        var payload = new JoinHouseholdDto(invitationCode);
        var response = await _httpClient.PutAsJsonAsync(HouseholdConstants.ApiEndpointJoinHousehold, payload);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to join household. Status: {response.StatusCode}");
            return false;
        }

        var tokens = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        if (tokens != null)
        {
            await _authService.UpdateAccessTokenAsync(tokens.AccessToken);
            return true;
        }

        return false;
    }
}

