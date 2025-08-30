
using AutoMapper;
using ChoreBuddies.Backend.Chores;

namespace ChoreBuddies.Backend.features.Chores
{
	public class ChoresService : IChoresService
	{
		private readonly IMapper _mapper;
		IEnumerable<Chore> mockChores = new List<Chore>
	   {
			new Chore(
				 Guid.NewGuid().ToString(),
				 "Clean Kitchen",
				 "Wipe counters, sweep floor, and take out trash",
				 "Alice",
				 DateTime.UtcNow.AddDays(2),
				 Status.Completed,
				 "Kitchen",
				 10
			),
			new Chore(
				 Guid.NewGuid().ToString(),
				"Vacuum Living Room",
				 "Vacuum carpet and dust surfaces",
				"Bob",
				DateTime.UtcNow.AddDays(1),
				Status.Assigned,
				 "Living Room",
				 15
			),
			new Chore(
				 Guid.NewGuid().ToString(),
				 "Laundry",
				"Wash, dry, and fold clothes",
				 null,
				DateTime.UtcNow.AddDays(3),
				Status.Unassigned,
				 "Laundry Room",
				20
			)
		};

		public ChoresService(IMapper mapper) => _mapper = mapper;

		public IEnumerable<ChoreOverviewDto> GetChores()
		{
			return mockChores.Select((t) => _mapper.Map<ChoreOverviewDto>(t));
		}

		public ChoreDto GetChoreDetails(string id)
		{
			return _mapper.Map<ChoreDto>(mockChores.First(t => t.Id == id));
		}
	}
}
