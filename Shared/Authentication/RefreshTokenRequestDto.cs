namespace Shared.Authentication;

public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);
