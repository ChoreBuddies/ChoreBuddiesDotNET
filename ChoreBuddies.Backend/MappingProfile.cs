using AutoMapper;

using ChoreBuddies.Backend.Domain;
using ChoreBuddies_SharedModels.Chores;
using ChoreBuddies_SharedModels.DefalutChores;


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