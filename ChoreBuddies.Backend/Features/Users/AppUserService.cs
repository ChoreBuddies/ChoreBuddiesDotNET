using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Shared.Authentication;
using Shared.Users;

namespace ChoreBuddies.Backend.Features.Users;

public interface IAppUserService
{
    public Task<AppUser?> GetUserByEmailAsync(string email);

    public Task<AppUser?> GetUserByIdAsync(int id);
    public Task<bool> ClearFcmTokenAsync(int userId);
    public Task<IEnumerable<AppUser>> GetUntrackedUsersByIdAsync(IEnumerable<int> ids);

    public Task<bool> UpdateUserAsync(int userId, UpdateAppUserDto userDto);
    public Task<bool> UpdateFcmTokenAsync(int userId, UpdateFcmTokenDto updateFcmTokenDto);

    public Task<ICollection<AppUserRoleDto>> GetUsersHouseholdMembersWithRolesAsync(int userId);
    public Task<ICollection<AppUser>> GetUsersHouseholdMembersAsync(int userId);
    public Task<ICollection<AppUser>> GetUsersHouseholdAdultsAsync(int userId);
    public Task<ICollection<AppUser>> GetUsersHouseholdChildrensAsync(int userId);

    public Task<int> GetUserPointsCountAsync(int userId);
    public Task<bool> AddPointsToUser(int userId, int pointsCount);
    public Task<bool> RemovePointsFromUser(int userId, int pointsCount);
    public Task<bool> UpdateUserRoleAsync(int userId, string roleName);
    public Task<IEnumerable<String>> GetAvailableRolesAsync();

}

public class AppUserService(IAppUserRepository userRepository, UserManager<AppUser> userManager, RoleManager<IdentityRole<int>> roleManager) : IAppUserService
{
    private readonly IAppUserRepository _userRepository = userRepository;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager = roleManager;

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetUserByEmailAsync(email);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateAppUserDto userDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return false;

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.DateOfBirth = userDto.DateOfBirth;
        user.UserName = userDto.UserName;
        user.Email = userDto.Email; // Consider adding email confirmation logic - issue #34

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddPointsToUser(int userId, int pointsCount)
    {
        if (pointsCount < 0) return false;
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null) return false;
        if (pointsCount == 0) return true;
        user.PointsCount += pointsCount;
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePointsFromUser(int userId, int pointsCount)
    {
        if (pointsCount < 0) return false;
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null || user.PointsCount < pointsCount) return false;
        if (pointsCount == 0) return true;
        user.PointsCount -= pointsCount;
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateFcmTokenAsync(int userId, UpdateFcmTokenDto updateFcmTokenDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return false;

        user.FcmToken = updateFcmTokenDto.Token;

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
    public async Task<ICollection<AppUserRoleDto>> GetUsersHouseholdMembersWithRolesAsync(int userId)
    {
        var user = await _userRepository.GetUserWithHouseholdByIdAsync(userId);
        if (user == null) return [];
        if (user.Household is null)
        {
            throw new ArgumentException("User doesn't belong to any household");
        }
        var result = new List<AppUserRoleDto>();
        foreach (var householdUser in user.Household.Users)
        {
            var role = (await _userManager.GetRolesAsync(householdUser)).SingleOrDefault();
            result.Add(new(householdUser.Id, householdUser?.UserName ?? String.Empty, role!));
        }
        return result;
    }

    public async Task<ICollection<AppUser>> GetUsersHouseholdMembersAsync(int userId)
    {
        var user = await _userRepository.GetUserWithHouseholdByIdAsync(userId);
        if (user == null) return [];
        if (user.Household is null)
        {
            throw new ArgumentException("User doesn't belong to any household");
        }
        return user.Household.Users;
    }

    public async Task<ICollection<AppUser>> GetUsersHouseholdAdultsAsync(int userId)
    {
        var members = await GetUsersHouseholdMembersAsync(userId);

        var adultIds = (await _userManager.GetUsersInRoleAsync(AuthConstants.RoleAdult))
            .Select(u => u.Id)
            .ToHashSet();

        return members
            .Where(m => adultIds.Contains(m.Id))
            .ToList();
    }
    public async Task<ICollection<AppUser>> GetUsersHouseholdChildrensAsync(int userId)
    {
        var members = await GetUsersHouseholdMembersAsync(userId);

        var childrenIds = (await _userManager.GetUsersInRoleAsync(AuthConstants.RoleChild))
            .Select(u => u.Id)
            .ToHashSet();

        return members
            .Where(m => childrenIds.Contains(m.Id))
            .ToList();
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User with id {userId} not found.");

        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new InvalidOperationException($"Role '{roleName}' does not exist.");

        var currentRole = (await _userManager.GetRolesAsync(user)).SingleOrDefault();

        if (currentRole == roleName)
            return true;

        if (currentRole != null)
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRole);
            if (!removeResult.Succeeded)
                return false;
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleName);
        return addResult.Succeeded;
    }

    public async Task<IEnumerable<AppUser>> GetUntrackedUsersByIdAsync(IEnumerable<int> ids)
    {
        return await _userRepository.GetUntrackedUsersByIdAsync(ids);
    }

    public async Task<IEnumerable<string>> GetAvailableRolesAsync()
    {
        var roles = _roleManager.Roles;

        return await Task.FromResult(roles.Select(r => r.Name!));
    }

    public async Task<int> GetUserPointsCountAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException($"User with id {userId} not found.");
        }
        return user.PointsCount;
    }

    public async Task<bool> ClearFcmTokenAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return false;

        user.FcmToken = null;

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}

