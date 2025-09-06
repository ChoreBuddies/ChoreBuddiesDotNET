namespace ChoreBuddies_SharedModels.Authentication;

public record AuthResultDto(
    string JwtToken,
    DateTime JwtTokenExpirationDate,
    string RefreshToken,
    DateTime RefreshTokenExpirationDate
);