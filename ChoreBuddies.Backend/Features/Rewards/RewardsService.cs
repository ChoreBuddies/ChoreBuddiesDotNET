using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Shared.Chores;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.Rewards;

public class RewardsService : IRewardsService
{
    private readonly IRewardsRepository _repository;
    private readonly IMapper _mapper;
    public RewardsService(IMapper mapper, IRewardsRepository rewardsRepository)
    {
        _mapper = mapper;
        _repository = rewardsRepository;
    }
    public async Task<RewardDto?> CreateRewardAsync(CreateRewardDto createRewardDto)
    {
        var newReward = _mapper.Map<Reward>(createRewardDto);
        var reward = await _repository.CreateRewardAsync(newReward);
        return _mapper.Map<RewardDto>(reward);
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
