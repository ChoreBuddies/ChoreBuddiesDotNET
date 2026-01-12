using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.RedeemRewards;

public interface IRedeemedRewardsRepository
{
    // Redeem
    public Task<RedeemedReward?> RedeemRewardAsync(RedeemedReward redeemedReward);
    // Get redeemed reward
    public Task<RedeemedReward?> GetRedeemedRewardAsync(int redeemedRewardId);
    // Update
    public Task<RedeemedReward?> UpdateRedeemedRewardAsync(RedeemedReward redeemedReward);
    // Get User's Redeemed
    public Task<ICollection<RedeemedReward>> GetUsersRedeemedRewardsAsync(int userId);
    // Get Household's Redeemed
    public Task<ICollection<RedeemedReward>> GetHouseholdsRedeemedRewardsAsync(int householdId);
    // Get Household's Redeemed Unfulfilled AS A QUERY
    public IQueryable<RedeemedReward> GetHouseholdsRedeemedRewardsQueryAsync(int householdId);
}
public class RedeemedRewardsRepository(ChoreBuddiesDbContext dbContext) : IRedeemedRewardsRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;
    public async Task<RedeemedReward?> RedeemRewardAsync(RedeemedReward redeemedReward)
    {
        var newRedeemedReward = await _dbContext.RedeemedRewards.AddAsync(redeemedReward);
        await _dbContext.SaveChangesAsync();
        return newRedeemedReward.Entity;
    }
    public async Task<RedeemedReward?> GetRedeemedRewardAsync(int redeemedRewardId)
    {
        return await _dbContext.RedeemedRewards.FindAsync(redeemedRewardId);
    }
    public async Task<RedeemedReward?> UpdateRedeemedRewardAsync(RedeemedReward redeemedReward)
    {
        _dbContext.RedeemedRewards.Update(redeemedReward);
        await _dbContext.SaveChangesAsync();
        return redeemedReward;
    }

    public async Task<ICollection<RedeemedReward>> GetUsersRedeemedRewardsAsync(int userId)
    {
        var user = await _dbContext.Users.Include(u => u.RedeemedRewards).FirstAsync(u => u.Id == userId);
        return user?.RedeemedRewards ?? [];
    }

    public async Task<ICollection<RedeemedReward>> GetHouseholdsRedeemedRewardsAsync(int householdId)
    {
        var household = await _dbContext.Households.Include(u => u.RedeemedRewards).FirstAsync(u => u.Id == householdId);
        return household?.RedeemedRewards ?? [];
    }

    public IQueryable<RedeemedReward> GetHouseholdsRedeemedRewardsQueryAsync(int householdId)
    {
        return _dbContext.RedeemedRewards.Where(r => r.HouseholdId == householdId && !r.IsFulfilled).AsNoTracking();
    }
}
