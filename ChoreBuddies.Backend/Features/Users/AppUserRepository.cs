using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Database;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Users;

public interface IAppUserRepository
{
    public Task<AppUser?> GetUserByEmailAsync(string email);

    public Task<AppUser?> GetUserByIdAsync(int id);

    public Task UpdateUserAsync(AppUser appUser);

    public Task SaveChangesAsync();
}

public class AppUserRepository(ChoreBuddiesDbContext dbContext) : IAppUserRepository
{
    private readonly ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateUserAsync(AppUser appUser)
    {
        _dbContext.ApplicationUsers.Update(appUser);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();

}
