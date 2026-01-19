using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.PredefinedChores;

public interface IPredefinedChoreRepository
{
    public Task<IEnumerable<PredefinedChore>> GetAllPredefinedChoreAsync();
    public Task<IEnumerable<PredefinedChore>> GetPredefinedChoresAsync(List<int> predefinedChoreIds);
}

public class PredefinedChoreRepository(ChoreBuddiesDbContext dbContext) : IPredefinedChoreRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<IEnumerable<PredefinedChore>> GetAllPredefinedChoreAsync()
    {
        return await _dbContext.PredefinedChores.ToListAsync();
    }

    public async Task<IEnumerable<PredefinedChore>> GetPredefinedChoresAsync(List<int> predefinedChoreIds)
    {
        return await _dbContext.PredefinedChores.Where(x => predefinedChoreIds.Contains(x.Id)).ToListAsync();
    }
}
