using ChoreBuddies.Backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Authentication;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChoreBuddies.Backend.Infrastructure.Authentication;

public interface ITokenService
{
    public Task<string> CreateAccessTokenAsync(AppUser user);
    public string CreateRefreshToken();
    public Task<AuthResponseDto> RefreshAccessToken(string accessToken, string refreshToken);
    public DateTime GetAccessTokenExpiration();
    public DateTime GetRefreshTokenExpiration();
    public int GetUserIdFromToken(ClaimsPrincipal claims);

    public string? GetUserNameFromToken(ClaimsPrincipal claims);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<AppUser> _userManager;
    private readonly SymmetricSecurityKey _key;
    private readonly TimeProvider _timeProvider;

    public TokenService(IConfiguration config, UserManager<AppUser> userManager, TimeProvider timeProvider)
    {
        _config = config;
        _userManager = userManager;
        // Get the secret key and create a signing credential
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
        _timeProvider = timeProvider;
    }

    public async Task<string> CreateAccessTokenAsync(AppUser user)
    {
        // Create claims for the token
        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
                    new Claim(ClaimTypes.Surname, user.LastName ?? ""),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(AuthConstants.JwtHouseholdId, user.HouseholdId?.ToString() ?? ""),
                };

        // Add user roles to the claims
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Create signing credentials
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512);

        // Describe the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = GetAccessTokenExpiration(),
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

    public async Task<AuthResponseDto> RefreshAccessToken(string expiredAccessToken, string refreshToken)
    {
        // 1. Get principal from expired token (ignore validation lifetime)
        var principal = GetPrincipalFromExpiredToken(expiredAccessToken);
        if (principal == null)
        {
            throw new SecurityTokenException("Invalid token");
        }

        // 2. Get user ID from claims (using NameId claim which contains the user's ID)
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new SecurityTokenException("Invalid token claims");
        }

        // 3. Find user in database
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= _timeProvider.GetUtcNow().DateTime)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // 4. Create new tokens
        var newAccessToken = await CreateAccessTokenAsync(user);
        var newRefreshToken = CreateRefreshToken();

        // 5. Update user with new refresh token (refresh token rotation)
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = GetRefreshTokenExpiration();
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto(newAccessToken, newRefreshToken);
    }

    public DateTime GetAccessTokenExpiration() =>
        _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(
            Convert.ToDouble(_config["JwtSettings:AccessTokenExpirationMinutes"], CultureInfo.InvariantCulture)
        );

    public DateTime GetRefreshTokenExpiration() =>
        _timeProvider.GetUtcNow().DateTime.AddDays(Convert.ToDouble(_config["JwtSettings:RefreshTokenExpirationDays"]));

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _config["JwtSettings:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _config["JwtSettings:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = false // IMPORTANT: we want to validate the token even if it's expired
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            // Additional validation: check if the token uses the expected algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public int GetUserIdFromToken(ClaimsPrincipal claims)
    {
        var userIdClaim = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException("Invalid user identifier.");
        }
        return userId;
    }

    public string? GetUserNameFromToken(ClaimsPrincipal claims)
    {
        return claims.FindFirstValue(JwtRegisteredClaimNames.Name);
    }
}
