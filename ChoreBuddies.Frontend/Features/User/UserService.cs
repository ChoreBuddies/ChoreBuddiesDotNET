using ChoreBuddies.Frontend.Utilities;
using Shared.Users;

namespace ChoreBuddies.Frontend.Features.User;

public interface IUserService
{
    public Task<AppUserDto?> GetCurrentUserAsync();
    public Task<AppUserDto?> GetUserByIdAsync(int userId);
    public Task<bool> UpdateUserAsync(AppUserDto user);
    public Task<bool> UpdateUserRoleAsync(int userId, string roleName);
    public Task<IEnumerable<AppUserRoleDto>?> GetMyHouseholdMembersWithRoleAsync();
    public Task<IEnumerable<AppUserDto>?> GetMyHouseholdMembersAsync();
    public Task<IEnumerable<String>> GetAvailableRolesAsync();
}

public class UserService : IUserService
{
    private readonly HttpClientUtils _httpClientUtils;

    public UserService(HttpClientUtils httpClientUtils)
    {
        _httpClientUtils = httpClientUtils;
    }

    public async Task<IEnumerable<string>> GetAvailableRolesAsync()
    {
        var roles = await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<IEnumerable<string>>(UserConstants.ApiEndpointRole, true)
        );
        return roles ?? Enumerable.Empty<string>();
    }

    public async Task<AppUserDto?> GetCurrentUserAsync()
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<AppUserDto>(UserConstants.ApiEndpointMe, true)
        );
    }
    public async Task<AppUserDto?> GetUserByIdAsync(int userId)
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<AppUserDto>($"{UserConstants.ApiEndpointUser}//{userId}", true)
        );
    }

    public async Task<IEnumerable<AppUserRoleDto>?> GetMyHouseholdMembersWithRoleAsync()
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<IEnumerable<AppUserRoleDto>>($"{UserConstants.ApiEndpointMyHouseholdMembers}{true}", true)
            );
    }
    public async Task<IEnumerable<AppUserDto>?> GetMyHouseholdMembersAsync()
    {
        return await _httpClientUtils.TryRequestAsync(
            () => _httpClientUtils.GetAsync<IEnumerable<AppUserDto>>(UserConstants.ApiEndpointMyHouseholdMembers, true)
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

    public async Task<bool> UpdateUserRoleAsync(int userId, string roleName)
    {
        var updateRoleDto = new UpdateRoleDto(userId, roleName);
        return await _httpClientUtils.TryRequestAsync(async () =>
        {
            await _httpClientUtils.PutAsync(UserConstants.ApiEndpointRole, updateRoleDto, true);
            return true;
        });
    }
}
