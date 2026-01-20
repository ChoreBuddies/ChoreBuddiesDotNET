using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.RedeemRewards;
using ChoreBuddies.Backend.Features.Rewards;
using ChoreBuddies.Backend.Features.Users;
using Microsoft.EntityFrameworkCore;
using Shared.RedeemedRewards;

namespace ChoreBuddies.Backend.Features.RedeemedRewards;

public interface IRedeemedRewardsService
{
    // Redeem
    public Task<RedeemedRewardDto?> RedeemRewardAsync(int userId, int rewardId, bool isFulfilled);
    // Fulfill Reward
    public Task<bool> FulfillRewardAsync(int redeemedRewardId);
    // Get User's Redeemed
    public Task<ICollection<RedeemedRewardDto>> GetUsersRedeemedRewardsAsync(int userId);
    // Get Household's Redeemed
    public Task<ICollection<RedeemedRewardDto>> GetHouseholdsRedeemedRewardsAsync(int householdId);
    // Get Household's Redeemed but Unfulfilled
    public Task<ICollection<RedeemedRewardWithUserNameDto>> GetHouseholdsUnfulfilledRedeemedRewardsAsync(int householdId);
}

public class RedeemedRewardsService(IRedeemedRewardsRepository redeemedRewardsRepository,
    IRewardsService rewardsService,
    IAppUserService appUserService,
    INotificationService notificationService,
    IMapper mapper) : IRedeemedRewardsService
{
    private readonly IRedeemedRewardsRepository _redeemedRewardsRepository = redeemedRewardsRepository;
    private readonly IRewardsService _rewardsService = rewardsService;
    private readonly IMapper _mapper = mapper;
    private readonly IAppUserService _appUserService = appUserService;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<ICollection<RedeemedRewardDto>> GetHouseholdsRedeemedRewardsAsync(int householdId)
    {
        return _mapper.Map<List<RedeemedRewardDto>>(await _redeemedRewardsRepository.GetHouseholdsRedeemedRewardsAsync(householdId));
    }
    public async Task<ICollection<RedeemedRewardWithUserNameDto>> GetHouseholdsUnfulfilledRedeemedRewardsAsync(int householdId)
    {
        return await _redeemedRewardsRepository.GetHouseholdsRedeemedRewardsQueryAsync(householdId)
            .ProjectTo<RedeemedRewardWithUserNameDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ICollection<RedeemedRewardDto>> GetUsersRedeemedRewardsAsync(int userId)
    {
        return _mapper.Map<List<RedeemedRewardDto>>(await _redeemedRewardsRepository.GetUsersRedeemedRewardsAsync(userId));
    }
    public async Task<bool> FulfillRewardAsync(int redeemedRewardId)
    {
        var reward = await _redeemedRewardsRepository.GetRedeemedRewardAsync(redeemedRewardId);
        if (reward is not null && !reward.IsFulfilled)
        {
            reward.IsFulfilled = true;
            await _redeemedRewardsRepository.UpdateRedeemedRewardAsync(reward);
            return true;
        }
        return false;
    }

    public async Task<RedeemedRewardDto?> RedeemRewardAsync(int userId, int rewardId, bool isFulfilled)
    {
        var reward = await _rewardsService.GetRewardByIdAsync(rewardId);
        var user = await _appUserService.GetUserByIdAsync(userId);
        if (reward is null || user is null)
            return null;
        if (reward.QuantityAvailable <= 0)
            throw new InvalidOperationException("This reward cannot be redeemed");
        if (user.PointsCount < reward.Cost)
            throw new InvalidOperationException("User does not have enough points");
        if (!await _appUserService.RemovePointsFromUser(userId, reward.Cost))
            throw new Exception("There was an error while removing points from user");
        reward = await _rewardsService.UpdateRewardAsync(reward with { QuantityAvailable = reward.QuantityAvailable - 1 }) ??
            throw new Exception("There was an error while editing Quantity Available from reward");
        var redeemedReward = new RedeemedReward()
        {
            Name = reward.Name,
            Description = reward.Description,
            UserId = userId,
            User = user,
            HouseholdId = reward.HouseholdId,
            RedeemedDate = DateTime.Now,
            PointsSpent = reward.Cost,
            IsFulfilled = isFulfilled
        };
        var result = await _redeemedRewardsRepository.RedeemRewardAsync(redeemedReward);

        if (!isFulfilled && result is not null)
        {
            var adults = await _appUserService.GetUsersHouseholdAdultsAsync(userId);
            foreach (var adult in adults)
            {
                await _notificationService.SendNewRewardRequestNotificationAsync(adult.Id, result?.Id ?? -1, result?.Name ?? "REWARD", user?.UserName ?? "USER");
            }
        }
        return _mapper.Map<RedeemedRewardDto>(result);
    }
}
