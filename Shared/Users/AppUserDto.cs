namespace Shared.Users;

public record AppUserDto(int Id, string FirstName, string LastName, DateTime DateOfBirth, string UserName, string Email, int? householdId);
