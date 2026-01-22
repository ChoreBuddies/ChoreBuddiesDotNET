namespace ChoreBuddies.Backend.Features.Households.Exceptions;

[Serializable]
public class HouseholdDoesNotExistException(int id) : Exception($"Invalid HouseholdId {id}");

