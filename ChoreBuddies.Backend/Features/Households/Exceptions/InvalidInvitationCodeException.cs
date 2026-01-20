namespace ChoreBuddies.Backend.Features.Households.Exceptions;

[Serializable]
public class InvalidInvitationCodeException(string code) : Exception($"Invalid invitation code {code}");
