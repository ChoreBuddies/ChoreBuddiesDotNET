using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IDefaultChoreRepository
{
    public Task<ICollection<PredefinedChore>> GetAllDefaultChoreAsync();
}

public class DefaultChoreRepository(ChoreBuddiesDbContext dbContext) : IDefaultChoreRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<ICollection<PredefinedChore>> GetAllDefaultChoreAsync()
    {
        return await _dbContext.PredefinedChores.ToListAsync();
    }
}
