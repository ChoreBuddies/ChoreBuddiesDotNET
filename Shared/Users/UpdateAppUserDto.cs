namespace Shared.Users;

public record UpdateAppUserDto(int Id, string FirstName, string LastName, DateTime DateOfBirth, string UserName, string Email);
