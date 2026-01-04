namespace Shared.Authentication;

public record RegisterRequestDto(string Email, string Password, string FirstName, string LastName, DateTime DateOfBirth, string UserName);
