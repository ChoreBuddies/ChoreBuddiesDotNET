using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.Households;
using Shared.Chores;
using Shared.DefalutChores;
using Shared.Notifications;
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
        CreateMap<DefaultChore, DefaultChoreDto>().ReverseMap();
        CreateMap<Household, HouseholdDto>().ReverseMap();
        CreateMap<Household, CreateHouseholdDto>().ReverseMap();
        CreateMap<AppUser, AppUserDto>().ReverseMap();
        CreateMap<NotificationPreference, NotificationPreferenceDto>().ReverseMap();
        CreateMap<ScheduledChore, CreateScheduledChoreDto>().ReverseMap();
        CreateMap<ScheduledChore, ScheduledChoreDto>().ReverseMap();
        CreateMap<ScheduledChore, ScheduledChoreOverviewDto>().ReverseMap();
    }
}
