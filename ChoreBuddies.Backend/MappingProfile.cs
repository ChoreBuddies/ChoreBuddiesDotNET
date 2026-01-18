using AutoMapper;
using ChoreBuddies.Backend.Domain;
using Shared.Chores;
using Shared.DefalutChores;
using Shared.Households;
using Shared.Notifications;
using Shared.PredefinedRewards;
using Shared.RedeemedRewards;
using Shared.Rewards;
using Shared.ScheduledChores;
using Shared.Users;

namespace ChoreBuddies.Backend;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Chore, ChoreDto>().ReverseMap();
        CreateMap<Chore, ChoreOverviewDto>().ReverseMap();
        CreateMap<Chore, CreateChoreDto>().ReverseMap();
        CreateMap<PredefinedChore, PredefinedChoreDto>().ReverseMap();
        CreateMap<Household, HouseholdDto>().ReverseMap();
        CreateMap<Household, CreateHouseholdDto>().ReverseMap();
        CreateMap<AppUser, AppUserDto>().ReverseMap();
        CreateMap<AppUser, AppUserMinimalDto>().ReverseMap();
        CreateMap<NotificationPreference, NotificationPreferenceDto>().ReverseMap();
        CreateMap<Reward, RewardDto>().ReverseMap();
        CreateMap<Reward, CreateRewardDto>().ReverseMap();
        CreateMap<PredefinedReward, PredefinedRewardDto>().ReverseMap();
        CreateMap<RedeemedReward, RedeemedRewardDto>().ReverseMap();
        CreateMap<RedeemedReward, RedeemedRewardWithUserNameDto>()
        .ForMember(
            dest => dest.UserName,
            opt => opt.MapFrom(src => src.User!.UserName)
        );
        CreateMap<ScheduledChore, CreateScheduledChoreDto>().ReverseMap();
        CreateMap<ScheduledChore, ScheduledChoreDto>().ReverseMap();
        CreateMap<ScheduledChore, ScheduledChoreOverviewDto>().ReverseMap();
        CreateMap<ScheduledChore, ScheduledChoreTileViewDto>()
            .ForCtorParam(nameof(ScheduledChoreTileViewDto.UserName), opt => opt.MapFrom(src =>
                src.User != null ? src.User.UserName : null));
    }
}
