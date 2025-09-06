namespace ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;

[Serializable]
public class UserAlreadyExistsException(string email) : Exception($"User with email {email} already exists");