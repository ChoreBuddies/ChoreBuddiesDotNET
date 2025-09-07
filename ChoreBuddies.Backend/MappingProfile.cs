using AutoMapper;

using ChoreBuddies.Backend.Chores;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.DefaultChores;
using ChoreBuddies.Backend.Features.Households;

namespace ChoreBuddies.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Chore, ChoreDto>().ReverseMap();
            CreateMap<Chore, ChoreOverviewDto>().ReverseMap();
            CreateMap<DefaultChore, DefaultChoreDto>().ReverseMap();
            CreateMap<Household, HouseholdDto>().ReverseMap();
            CreateMap<Household, CreateHouseholdDto>().ReverseMap();
        }
    }
}