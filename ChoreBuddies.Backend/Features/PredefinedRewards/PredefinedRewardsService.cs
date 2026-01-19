using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Shared.PredefinedRewards;

namespace ChoreBuddies.Backend.Features.PredefinedRewards;

public interface IPredefinedRewardsService
{
    public Task<IEnumerable<PredefinedRewardDto>> GetAllPredefinedRewardsAsync();
    public Task<IEnumerable<PredefinedRewardDto>> GetPredefinedRewardsAsync(List<int> predefinedRewardIds);
}
public class PredefinedRewardsService(IPredefinedRewardsRepository predefinedRewardsRepository, IMapper mapper) : IPredefinedRewardsService
{
    private readonly IPredefinedRewardsRepository _repository = predefinedRewardsRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<PredefinedRewardDto>> GetAllPredefinedRewardsAsync()
    {
        return _mapper.Map<List<PredefinedRewardDto>>(await _repository.GetAllPredefinedRewardsAsync());
    }

    public async Task<IEnumerable<PredefinedRewardDto>> GetPredefinedRewardsAsync(List<int> predefinedRewardIds)
    {
        return _mapper.Map<List<PredefinedRewardDto>>(await _repository.GetPredefinedRewardsAsync(predefinedRewardIds));
    }
}
