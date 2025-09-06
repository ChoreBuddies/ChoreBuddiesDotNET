namespace ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;

[Serializable]
public class RegistrationFailedException(IEnumerable<string> errors) :
    Exception($"Registration failed with following errors: {string.Join(Environment.NewLine, errors)}");
