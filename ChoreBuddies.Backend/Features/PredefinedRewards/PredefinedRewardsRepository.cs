using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.PredefinedRewards;

public interface IPredefinedRewardsRepository
{
    public Task<ICollection<PredefinedReward>> GetAllPredefinedRewardsAsync();
}
public class PredefinedRewardsRepository(ChoreBuddiesDbContext dbContext) : IPredefinedRewardsRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<ICollection<PredefinedReward>> GetAllPredefinedRewardsAsync()
    {
        return await _dbContext.PredefinedRewards.ToListAsync();
    }
}
