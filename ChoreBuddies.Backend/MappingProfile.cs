using AutoMapper;
using ChoreBuddies.Backend.Chores;
using ChoreBuddies.Backend.features.Chores;

namespace ChoreBuddies.Backend
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Chore, ChoreDto>();
			CreateMap<ChoreDto, Chore>();
			CreateMap<Chore, ChoreOverviewDto>();
			CreateMap<ChoreOverviewDto, Chore>();
		}
	}
}
