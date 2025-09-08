using AutoMapper;

using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using Shared.Chores;
using Shared.DefalutChores;
using Shared.Users;

namespace ChoreBuddies.Backend;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Chore, ChoreDto>().ReverseMap();
        CreateMap<Chore, ChoreOverviewDto>().ReverseMap();
        CreateMap<DefaultChore, DefaultChoreDto>().ReverseMap();
        CreateMap<Household, HouseholdDto>().ReverseMap();
        CreateMap<Household, CreateHouseholdDto>().ReverseMap();
        CreateMap<AppUser, AppUserDto>().ReverseMap();
    }
}
