using ChoreBuddies_SharedModels.Authentication;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

public interface IAuthService
{
    public Task RegisterUser(RegisterRequestDto registerRequest);
    public Task<AuthResultDto> LoginUser(LoginRequestDto loginRequest);
}

public class AuthService : IAuthService
{
    public Task<AuthResultDto> LoginUser(LoginRequestDto loginRequest)
    {
        throw new NotImplementedException();
    }

    public Task RegisterUser(RegisterRequestDto registerRequest)
    {
        throw new NotImplementedException();
    }
}
