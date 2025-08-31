using AutoMapper;

using ChoreBuddies.Backend.Chores;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.DefaultChores;

namespace ChoreBuddies.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Chore, ChoreDto>().ReverseMap();
            CreateMap<Chore, ChoreOverviewDto>().ReverseMap();
            CreateMap<DefaultChore, DefaultChoreDto>().ReverseMap();
        }
    }
}