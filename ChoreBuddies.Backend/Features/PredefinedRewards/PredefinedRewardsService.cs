using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Shared.PredefinedRewards;

namespace ChoreBuddies.Backend.Features.PredefinedRewards;

public interface IPredefinedRewardsService
{
    public Task<ICollection<PredefinedRewardDto>> GetAllPredefinedRewardsAsync();
}
public class PredefinedRewardsService(IPredefinedRewardsRepository predefinedRewardsRepository, IMapper mapper) : IPredefinedRewardsService
{
    private readonly IPredefinedRewardsRepository _predefinedRewardsRepository = predefinedRewardsRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<ICollection<PredefinedRewardDto>> GetAllPredefinedRewardsAsync()
    {
        return _mapper.Map<List<PredefinedRewardDto>>(await _predefinedRewardsRepository.GetAllPredefinedRewardsAsync());
    }
}
