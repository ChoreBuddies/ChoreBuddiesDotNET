using ChoreBuddies.Backend.Domain;
using Shared.Users;

namespace ChoreBuddies.Backend.Features.Users;

public interface IAppUserService
{
    public Task<AppUser?> GetUserByEmailAsync(string email);

    public Task<AppUser?> GetUserByIdAsync(int id);

    public Task<bool> UpdateUserAsync(int userId, UpdateAppUserDto userDto);
    public Task<bool> UpdateFcmTokenAsync(int userId, UpdateFcmTokenDto updateFcmTokenDto);

    public Task<ICollection<AppUser>> GetUsersHouseholdMembersAsync(int userId);
    public Task<ICollection<AppUser>> GetUsersHouseholdParentsAsync(int userId);
    public Task<bool> AddPointsToUser(int userId, int pointsCount);
    public Task<bool> RemovePointsFromUser(int userId, int pointsCount);

}

public class AppUserService(IAppUserRepository userRepository) : IAppUserService
{
    private readonly IAppUserRepository _userRepository = userRepository;

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
        if (pointsCount <= 0) return false;
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null) return false;
        user.PointsCount += pointsCount;
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePointsFromUser(int userId, int pointsCount)
    {
        if (pointsCount <= 0) return false;
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null || user.PointsCount < pointsCount) return false;
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

    public async Task<ICollection<AppUser>> GetUsersHouseholdParentsAsync(int userId)
    {
        var allMembers = await GetUsersHouseholdMembersAsync(userId); // TODO: fix when userTypes get defined
        //return allMembers.Where(v => v.userType == UserType.Adult);
        return allMembers;
    }
}

