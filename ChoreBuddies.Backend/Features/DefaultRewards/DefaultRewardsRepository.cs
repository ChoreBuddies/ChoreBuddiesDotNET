using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.DefaultRewards;

public interface IDefaultRewardsRepository
{
    public Task<ICollection<PredefinedReward>> GetAllDefaultRewardsAsync();
}
public class DefaultRewardsRepository(ChoreBuddiesDbContext dbContext) : IDefaultRewardsRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<ICollection<PredefinedReward>> GetAllDefaultRewardsAsync()
    {
        return await _dbContext.PredefinedRewards.ToListAsync();
    }
}
