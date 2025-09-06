using AutoMapper;

using ChoreBuddies.Backend.Domain;
using ChoreBuddies_SharedModels.Chores;
using ChoreBuddies_SharedModels.DefalutChores;
using ChoreBuddies_SharedModels.Users;

namespace ChoreBuddies.Backend;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Chore, ChoreDto>().ReverseMap();
        CreateMap<Chore, ChoreOverviewDto>().ReverseMap();
        CreateMap<DefaultChore, DefaultChoreDto>().ReverseMap();
        CreateMap<AppUser, AppUserDto>().ReverseMap();
    }
}
