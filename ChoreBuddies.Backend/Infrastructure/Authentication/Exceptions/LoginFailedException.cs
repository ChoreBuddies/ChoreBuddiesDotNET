namespace ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;

[Serializable]
public class LoginFailedException(string email) : Exception($"Invalid email: {email} or password");
