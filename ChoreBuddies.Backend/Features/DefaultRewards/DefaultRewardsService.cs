using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Shared.Rewards;

namespace ChoreBuddies.Backend.Features.DefaultRewards;

public interface IDefaultRewardsService
{
    public Task<ICollection<DefaultRewardDto>> GetAllDefaultRewardsAsync();
}
public class DefaultRewardsService(IDefaultRewardsRepository defaultRewardsRepository, IMapper mapper) : IDefaultRewardsService
{
    private readonly IDefaultRewardsRepository _defaultRewardsRepository = defaultRewardsRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ICollection<DefaultRewardDto>> GetAllDefaultRewardsAsync()
    {
        return _mapper.Map<List<DefaultRewardDto>>(await _defaultRewardsRepository.GetAllDefaultRewardsAsync());
    }
}
