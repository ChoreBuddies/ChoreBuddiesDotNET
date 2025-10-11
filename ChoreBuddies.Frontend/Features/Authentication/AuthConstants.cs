namespace ChoreBuddies.Frontend.Features.Authentication;

public static class AuthConstants
{
    public const string AuthTokenKey = "authToken";
    public const string RefreshToken = "refreshToken";
    public const string Jwt = "jwt";
    public const string Bearer = "Bearer";
    public const string AuthorizedClient = "AuthorizedClient";
    public const string UnauthorizedClient = "UnauthorizedClient";

    public const string ApiEndpointRefresh = "api/v1/auth/refresh";
    public const string ApiEndpointRevoke = "api/v1/auth/revoke";
    public const string ApiEndpointLogin = "api/v1/auth/login";
    public const string ApiEndpointSignup = "api/v1/auth/signup";
}
