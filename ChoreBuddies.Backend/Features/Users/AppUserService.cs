using ChoreBuddies.Backend.Domain;
using Shared.Users;

namespace ChoreBuddies.Backend.Features.Users;

public interface IAppUserService
{
    public Task<AppUser?> GetUserByEmailAsync(string email);

    public Task<AppUser?> GetUserByIdAsync(int id);

    public Task<bool> UpdateUserAsync(int userId, UpdateAppUserDto userDto);
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

        user.UserName = userDto.UserName;
        user.Email = userDto.Email; // Consider adding email confirmation logic - issue #34

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}

