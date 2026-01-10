using Shared.Households;
using ChoreBuddies.Frontend.Features.Authentication;
using Shared.Authentication;
using System.Net.Http.Json;
using ChoreBuddies.Frontend.Utilities;
using ChoreBuddies.Frontend.Features.Chores;
using Shared.Chores;

namespace ChoreBuddies.Frontend.Features.Household;

public interface IHouseholdService
{
    public Task<bool> JoinHouseholdAsync(string invitationCode);
    public Task<HouseholdDto?> GetHouseholdByIdAsync(int id);
}

public class HouseholdService : IHouseholdService
{
    private readonly HttpClientUtils _httpUtils;
    private readonly IAuthService _authService;

    public HouseholdService(HttpClientUtils httpClient, IAuthService authService)
    {
        _httpUtils = httpClient;
        _authService = authService;
    }

    public async Task<HouseholdDto?> GetHouseholdByIdAsync(int id)
    {
        var result = await _httpUtils.TryRequestAsync(async () =>
        {
            return await _httpUtils.GetAsync<HouseholdDto?>(
                $"{HouseholdConstants.ApiEndpointGetHousehold}{id}",
                authorized: true
            );
        });

        return result ?? null;
    }
    public async Task<HouseholdDto?> CreateHouseholdAsync(CreateHouseholdDto householdDto)
    {
        var result = await _httpUtils.TryRequestAsync(async () =>
        {
            return await _httpUtils.PostAsync<CreateHouseholdDto, HouseholdDto?>(
                HouseholdConstants.ApiEndpointCreateHousehold,
                householdDto,
                authorized: true
            );
        });
        return result ?? null;
    }
    public async Task<HouseholdDto?> UpdateHouseholdAsync(HouseholdDto householdDto)
    {
        var result = await _httpUtils.TryRequestAsync(async () =>
        {
            return await _httpUtils.PostAsync<HouseholdDto, HouseholdDto?>(
                HouseholdConstants.ApiEndpointUpdateHousehold + householdDto.Id,
                householdDto,
                authorized: true
            );
        });
        return result ?? null;
    }

    public async Task<bool> JoinHouseholdAsync(string invitationCode)
    {
        var payload = new JoinHouseholdDto(invitationCode);
        var tokens = await _httpUtils.PutAsync<JoinHouseholdDto, AuthResponseDto>(HouseholdConstants.ApiEndpointJoinHousehold, payload);

        if (tokens != null)
        {
            await _authService.UpdateAccessTokenAsync(tokens.AccessToken);
            return true;
        }

        return false;
    }

}

