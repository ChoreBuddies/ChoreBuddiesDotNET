using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.PredefinedRewards;

public interface IPredefinedRewardsRepository
{
    public Task<IEnumerable<PredefinedReward>> GetAllPredefinedRewardsAsync();
    public Task<IEnumerable<PredefinedReward>> GetPredefinedRewardsAsync(List<int> predefinedRewardIds);
}
public class PredefinedRewardsRepository(ChoreBuddiesDbContext dbContext) : IPredefinedRewardsRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<IEnumerable<PredefinedReward>> GetAllPredefinedRewardsAsync()
    {
        return await _dbContext.PredefinedRewards.ToListAsync();
    }

    public async Task<IEnumerable<PredefinedReward>> GetPredefinedRewardsAsync(List<int> predefinedRewardIds)
    {
        return await _dbContext.PredefinedRewards.Where(x => predefinedRewardIds.Contains(x.Id)).ToListAsync();
    }
}
