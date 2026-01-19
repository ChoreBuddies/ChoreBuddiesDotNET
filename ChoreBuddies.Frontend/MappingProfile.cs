using AutoMapper;
using ChoreBuddies.Frontend.Features.Chores;
using Shared.Chores;
using Shared.ScheduledChores;

namespace ChoreBuddies.Frontend;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ChoreViewModel, CreateChoreDto>()
                .ConstructUsing(src => new CreateChoreDto(
                    src.Name,
                    src.Description ?? string.Empty,
                    src.UserId,
                    src.HouseholdId,
                    src.DueDate,
                    null,
                    src.Room,
                    src.RewardPointsCount
                ));

        CreateMap<ChoreViewModel, CreateScheduledChoreDto>()
            .ConstructUsing(src => new CreateScheduledChoreDto(
                src.Name,
                src.Description ?? string.Empty,
                src.UserId,
                src.Room,
                src.RewardPointsCount,
                src.Frequency,
                src.ChoreDuration,
                src.EveryX
            )); ;
        CreateMap<ChoreDto, ChoreViewModel>();
        CreateMap<ChoreViewModel, ChoreDto>()
            .ConstructUsing(src => new ChoreDto(
                src.Id ?? 0,
                src.Name,
                src.Description ?? string.Empty,
                src.UserId,
                src.HouseholdId,
                src.DueDate,
                null,
                src.Room,
                src.RewardPointsCount
            ));
        CreateMap<ChoreViewModel, ScheduledChoreDto>().ReverseMap();
    }
}
