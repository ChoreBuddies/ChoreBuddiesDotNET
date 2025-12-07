using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.RedeemRewards;
using ChoreBuddies.Backend.Features.Rewards;
using ChoreBuddies.Backend.Features.Users;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.RedeemedRewards;

public class DefaultRewardsService(IRedeemedRewardsRepository redeemedRewardsRepository, 
    IRewardsRepository rewardsRepository, 
    IAppUserRepository appUserRepository,
    IMapper mapper) : IRedeemedRewardsService
{
    private readonly IRedeemedRewardsRepository _redeemedRewardsRepository = redeemedRewardsRepository;
    private readonly IRewardsRepository _rewardRepository = rewardsRepository;
    private readonly IAppUserRepository _appUserRepository = appUserRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ICollection<RedeemedRewardDto>> GetHouseholdsRedeemedRewardsAsync(int householdId)
    {
        return _mapper.Map<List<RedeemedRewardDto>>(await _redeemedRewardsRepository.GetUsersRedeemedRewardsAsync(householdId));
    }

    public async Task<ICollection<RedeemedRewardDto>> GetUsersRedeemedRewardsAsync(int userId)
    {
        return _mapper.Map<List<RedeemedRewardDto>>(await _redeemedRewardsRepository.GetUsersRedeemedRewardsAsync(userId));
    }

    public async Task<RedeemedRewardDto?> RedeemRewardAsync(int userId, int rewardId, bool isFulfilled)
    {
        var reward = await _rewardRepository.GetRewardByIdAsync(rewardId);
        var user = await _appUserRepository.GetUserByIdAsync(userId);
        if (reward is null || user is null)
            return null;
        if (user.PointsCount < reward.Cost)
            throw new InvalidOperationException("User does not have enough points");
        user.PointsCount -= reward.Cost;
        var redeemedReward = new RedeemedReward() { 
            Name = reward.Name, 
            Description = reward.Description,
            UserId = userId,
            User = user,
            HouseholdId = reward.HouseholdId,
            RedeemedDate = DateTime.Now,
            PointsSpent = reward.Cost,
            IsFulfilled = isFulfilled
        };
        return _mapper.Map<RedeemedRewardDto>(await _redeemedRewardsRepository.RedeemRewardAsync(redeemedReward));
    }
}
