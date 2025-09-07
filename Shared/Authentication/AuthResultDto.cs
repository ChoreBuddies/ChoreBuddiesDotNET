namespace Shared.Authentication;

public record AuthResultDto(
    string JwtToken,
    DateTime JwtTokenExpirationDate,
    string RefreshToken,
    DateTime RefreshTokenExpirationDate
);
