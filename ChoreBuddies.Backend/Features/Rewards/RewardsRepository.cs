using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreBuddies.Backend.Features.Rewards;

public interface IRewardsRepository
{
    // Create
    public Task<Reward?> CreateRewardAsync(Reward reward);
    // Read
    public Task<Reward?> GetRewardByIdAsync(int rewardId);
    // Update
    public Task<Reward?> UpdateRewardAsync(Reward reward);
    // Delete
    public Task<Reward?> DeleteRewardAsync(Reward reward);
    // Get Household's Rewards
    public Task<ICollection<Reward>?> GetHouseholdRewardsAsync(int householdId);
}
public class RewardsRepository(ChoreBuddiesDbContext dbContext) : IRewardsRepository
{
    private ChoreBuddiesDbContext _dbContext = dbContext;
    public async Task<Reward?> CreateRewardAsync(Reward reward)
    {
        var newReward = await _dbContext.Rewards.AddAsync(reward);
        await _dbContext.SaveChangesAsync();
        return newReward.Entity;
    }

    public async Task<Reward?> DeleteRewardAsync(Reward reward)
    {
        var deletedReward = _dbContext.Rewards.Remove(reward);
        await _dbContext.SaveChangesAsync();
        return deletedReward.Entity;

    }

    public async Task<ICollection<Reward>?> GetHouseholdRewardsAsync(int householdId)
    {
        try
        {
            var household = await _dbContext.Households
                .Include(h => h.Rewards)
                .FirstOrDefaultAsync(h => h.Id == householdId);
            if (household is null || household.Rewards is null)
                return null;
            return household.Rewards;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public async Task<Reward?> GetRewardByIdAsync(int rewardId)
    {
        try
        {
            return await _dbContext.Rewards.FindAsync(rewardId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Reward?> UpdateRewardAsync(Reward reward)
    {
        var currentReward = await GetRewardByIdAsync(reward.Id);
        if (currentReward is null)
            return null;
        try
        {
            if(reward.Name is not null && reward.Name.Length > 0)
            {
                currentReward.Name = reward.Name;
            }
            if (reward.Description is not null && reward.Description.Length > 0)
            {
                currentReward.Description = reward.Description;
            }
            if (reward.Cost > 0)
            {
                currentReward.Cost = reward.Cost;
            }
            if (reward.QuantityAvailable >= 0)
            {
                currentReward.QuantityAvailable = reward.QuantityAvailable;
            }
        }
        catch
        {
            return null;
        }
        await _dbContext.SaveChangesAsync();
        return currentReward;
    }
}
