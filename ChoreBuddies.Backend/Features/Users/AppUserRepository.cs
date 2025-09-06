using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Database;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Users;

public interface IAppUserRepository
{
    public Task<AppUser?> GetUserByEmailAsync(string email);

    public Task<AppUser?> GetUserByIdAsync(int id);
}

public class AppUserRepository(ChoreBuddiesDbContext dbContext) : IAppUserRepository
{
    private readonly ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
