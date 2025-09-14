using System.Security.Claims;

namespace ChoreBuddies.Frontend.Features.Authentication;

public interface IAuthService
{
    Task<string> GetToken();
    Task<string> GetUserId();
    Task<string> GetUserEmail();
    Task<IEnumerable<Claim>> GetClaims();
    Task<bool> IsAuthenticated();
    Task Login(string token, string? refreshToken = null);
    Task Logout();
}

public class AuthService : IAuthService
{
    public IEnumerable<Claim> GetClaims()
    {
        throw new NotImplementedException();
    }

    public string GetToken()
    {
        throw new NotImplementedException();
    }

    public string GetUserEmail()
    {
        throw new NotImplementedException();
    }

    public string GetUserId()
    {
        throw new NotImplementedException();
    }

    public bool IsAuthenticated()
    {
        throw new NotImplementedException();
    }

    public Task Login(string token, string? refreshToken = null)
    {
        throw new NotImplementedException();
    }

    public Task Logout()
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<Claim>> IAuthService.GetClaims()
    {
        throw new NotImplementedException();
    }

    Task<string> IAuthService.GetToken()
    {
        throw new NotImplementedException();
    }

    Task<string> IAuthService.GetUserEmail()
    {
        throw new NotImplementedException();
    }

    Task<string> IAuthService.GetUserId()
    {
        throw new NotImplementedException();
    }

    Task<bool> IAuthService.IsAuthenticated()
    {
        throw new NotImplementedException();
    }
}
