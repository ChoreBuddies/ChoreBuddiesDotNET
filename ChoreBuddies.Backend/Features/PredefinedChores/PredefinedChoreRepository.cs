using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.DefaultChores;

public interface IPredefinedChoreRepository
{
    public Task<ICollection<PredefinedChore>> GetAllPredefinedChoreAsync();
}

public class PredefinedChoreRepository(ChoreBuddiesDbContext dbContext) : IPredefinedChoreRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<ICollection<PredefinedChore>> GetAllPredefinedChoreAsync()
    {
        return await _dbContext.PredefinedChores.ToListAsync();
    }
}
