using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Frontend.Features.Authentication;
using System.Net.Http.Json;

namespace ChoreBuddies.Frontend.Features.Household;

public interface IHouseholdService
{
    public Task<bool> JoinHouseholdAsync(string invitationCode);
}

public class HouseholdService : IHouseholdService
{
    private readonly HttpClient _httpClient;

    public HouseholdService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(AuthFrontendConstants.AuthorizedClient);
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

        return true;
    }
}

