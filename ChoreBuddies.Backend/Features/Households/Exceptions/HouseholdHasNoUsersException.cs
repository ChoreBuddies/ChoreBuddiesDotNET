namespace ChoreBuddies.Backend.Features.Households.Exceptions;
[Serializable]
public class HouseholdHasNoUsersException(int id) : Exception($"Household with id {id} has no users");
