using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.PredefinedRewards;
using Shared.Chores;
using Shared.PredefinedChores;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.Rewards;

public interface IRewardsService
{
    // Create
    public Task<RewardDto?> CreateRewardAsync(CreateRewardDto createRewardDto);
    // Create from predefiend
    public Task<IEnumerable<RewardDto>> AddPredefinedRewardsToHouseholdAsync(List<int> predefinedRewardIds, int householdId);
    // Read
    public Task<RewardDto?> GetRewardByIdAsync(int rewardId);
    // Update
    public Task<RewardDto?> UpdateRewardAsync(RewardDto rewardDto);
    // Delete
    public Task<RewardDto?> DeleteRewardAsync(int rewardId);
    // Get Household's Rewards
    public Task<ICollection<RewardDto>?> GetHouseholdRewardsAsync(int householdId);
}

public class RewardsService : IRewardsService
{
    private readonly IRewardsRepository _repository;
    private readonly IPredefinedRewardsService _predefinedRewardService;
    private readonly IMapper _mapper;
    public RewardsService(
        IMapper mapper,
        IRewardsRepository rewardsRepository,
        IPredefinedRewardsService predefinedRewardsService)
    {
        _mapper = mapper;
        _repository = rewardsRepository;
        _predefinedRewardService = predefinedRewardsService;
    }
    public async Task<RewardDto?> CreateRewardAsync(CreateRewardDto createRewardDto)
    {
        var newReward = _mapper.Map<Reward>(createRewardDto);
        var reward = await _repository.CreateRewardAsync(newReward);
        return _mapper.Map<RewardDto>(reward);
    }

    public async Task<IEnumerable<RewardDto>> AddPredefinedRewardsToHouseholdAsync(List<int> predefinedRewardIds, int householdId)
    {
        var predefinedRewards = await _predefinedRewardService.GetPredefinedRewardsAsync(predefinedRewardIds);

        var createdRewards = new List<Reward>();
        foreach (var p in predefinedRewards)
        {
            var newChore = new Reward(
                name: p.Name,
                description: p.Description,
                cost: p.Cost,
                quantityAvailable: p.QuantityAvailable,
                householdId: householdId
            );

            var createdReward = await _repository.CreateRewardAsync(newChore);
            if (createdReward is null)
                throw new Exception("Creating Reward from Predefined Reward Failed");

            createdRewards.Add(createdReward);
        }

        return _mapper.Map<List<RewardDto>>(createdRewards);
    }

    public async Task<RewardDto?> DeleteRewardAsync(int rewardId)
    {
        var reward = await _repository.GetRewardByIdAsync(rewardId);
        if (reward != null)
        {
            var resultReward = await _repository.DeleteRewardAsync(reward);
            return _mapper.Map<RewardDto>(resultReward);
        }
        else
        {
            throw new Exception("Reward not found");
        }
    }

    public async Task<ICollection<RewardDto>?> GetHouseholdRewardsAsync(int householdId)
    {
        return _mapper.Map<List<RewardDto>>(await _repository.GetHouseholdRewardsAsync(householdId));
    }

    public async Task<RewardDto?> GetRewardByIdAsync(int rewardId)
    {
        var reward = await _repository.GetRewardByIdAsync(rewardId);
        if (reward is null) throw new Exception("Reward not found");
        return _mapper.Map<RewardDto?>(reward);
    }

    public async Task<RewardDto?> UpdateRewardAsync(RewardDto rewardDto)
    {
        var reward = await _repository.GetRewardByIdAsync(rewardDto.Id);
        if (reward != null)
        {
            var newReward = _mapper.Map<Reward>(rewardDto);
            var resultReward = await _repository.UpdateRewardAsync(newReward);
            return _mapper.Map<RewardDto>(resultReward);
        }
        else
        {
            throw new Exception("Reward not found");
        }
    }

}
