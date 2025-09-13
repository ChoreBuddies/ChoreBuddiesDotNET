using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;
public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(AppUser user);
    string CreateRefreshToken();
    Task<string> RefreshAccessToken(string accessToken, string refreshToken);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<AppUser> _userManager;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config, UserManager<AppUser> userManager)
    {
        _config = config;
        _userManager = userManager;
        // Get the secret key and create a signing credential
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
    }

    public async Task<string> CreateAccessTokenAsync(AppUser user)
    {
        // Create claims for the token
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
            };

        // Add user roles to the claims
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Create signing credentials
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        // Describe the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_config["JwtSettings:AccessTokenExpirationMinutes"])),
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"],
            SigningCredentials = creds
        };

        // Create the token handler and write the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<string> RefreshAccessToken(string expiredAccessToken, string refreshToken)
    {
        // This is a simplified version. You would need to:
        // 1. Validate the expired token to get the user's ID (ignoring expiration)
        // 2. Retrieve the user from the database
        // 3. Verify the stored refresh token for that user matches the provided one and hasn't expired
        // 4. If valid, create a new access token and return it
        // 5. Optionally, issue a new refresh token (refresh token rotation)

        throw new NotImplementedException("Implement refresh token validation logic here.");
        // For a full implementation, you need a way to store refresh tokens (e.g., in your AppIdentityDbContext).
        await _userManager.GetRolesAsync(new AppUser());
    }
}
