namespace ChoreBuddies.Backend.Features.Households;

public record HouseholdDto(
    int Id,
    string Name,
    string Description,
    int OwnerId,
    DateTime CreatedDate
    );
