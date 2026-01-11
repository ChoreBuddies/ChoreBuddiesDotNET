using ChoreBuddies.Frontend.Utilities;
using Shared.Users;

namespace ChoreBuddies.Frontend.Features.User;

public interface IUserService
{
    public Task<AppUserDto?> GetCurrentUserAsync();
    public Task<bool> UpdateUserAsync(AppUserDto user);
    public Task<IEnumerable<AppUserRoleDto>?> GetMyHouseholdMembersAsync();
}

public class UserService : IUserService
{
    private readonly HttpClientUtils _httpClientUtils;

    public UserService(HttpClientUtils httpClientUtils)
    {
        _httpClientUtils = httpClientUtils;
    }

    public async Task<AppUserDto?> GetCurrentUserAsync()
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<AppUserDto>(UserConstants.ApiEndpointMe, true)
        );
    }

    public async Task<IEnumerable<AppUserRoleDto>?> GetMyHouseholdMembersAsync()
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<IEnumerable<AppUserRoleDto>>(UserConstants.ApiEndpointMyHouseholdMembers, true)
            );
    }

    public async Task<bool> UpdateUserAsync(AppUserDto user)
    {
        return await _httpClientUtils.TryRequestAsync(async () =>
        {
            await _httpClientUtils.PutAsync(UserConstants.ApiEndpointMe, user, true);
            return true;
        });
    }
}
