using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Claims;

namespace ChoreBuddies.Tests.Authentication;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly TokenService _tokenService;
    private readonly DateTimeOffset _fixedNow;
    public TokenServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        _timeProviderMock = new Mock<TimeProvider>();

        _configMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("super_secret_key_that_is_long_enough_for_sha512_12345_3489277893458793456789435673420");
        _configMock.Setup(c => c["JwtSettings:Issuer"]).Returns("ChoreBuddies");
        _configMock.Setup(c => c["JwtSettings:Audience"]).Returns("ChoreBuddiesUsers");
        _configMock.Setup(c => c["JwtSettings:AccessTokenExpirationMinutes"]).Returns("60");
        _configMock.Setup(c => c["JwtSettings:RefreshTokenExpirationDays"]).Returns("7");

        _fixedNow = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(_fixedNow);

        _tokenService = new TokenService(_configMock.Object, _userManagerMock.Object, _timeProviderMock.Object);
    }

    [Fact]
    public async Task CreateAccessTokenAsync_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new AppUser
        {
            Id = 1,
            Email = "test@test.com",
            UserName = "testuser",
            FirstName = "Jan",
            LastName = "Kowalski",
            HouseholdId = 10
        };
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Adult" });

        // Act
        var token = await _tokenService.CreateAccessTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateRefreshToken_ShouldReturnRandomString()
    {
        // Act
        var token1 = _tokenService.CreateRefreshToken();
        var token2 = _tokenService.CreateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var expiredToken = await CreateTestToken(1);
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null!);

        // Act & Assert
        await _tokenService.Invoking(s => s.RefreshAccessTokenAsync(expiredToken, "any-refresh-token"))
            .Should().ThrowAsync<SecurityTokenException>()
            .WithMessage("Invalid refresh token");
    }

    [Fact]
    public void GetUserIdFromToken_ShouldReturnCorrectId()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "123") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _tokenService.GetUserIdFromToken(principal);

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void GetHouseholdIdFromToken_ShouldThrowException_WhenClaimMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act & Assert
        _tokenService.Invoking(s => s.GetHouseholdIdFromToken(principal))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetAccessTokenExpiration_ShouldReturnCorrectExpirationTime()
    {
        // Act
        var expiration = _tokenService.GetAccessTokenExpiration();

        // Assert 
        expiration.Should().Be(_fixedNow.UtcDateTime.AddMinutes(60));
    }

    [Theory]
    [InlineData(ClaimTypes.Email, "user@example.com")]
    public void GetUserEmailFromToken_ShouldReturnCorrectEmail(string claimType, string expectedValue)
    {
        // Arrange
        var principal = CreatePrincipal(claimType, expectedValue);

        // Act
        var result = _tokenService.GetUserEmailFromToken(principal);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetFirstNameFromToken_ShouldReturnCorrectFirstName()
    {
        // Arrange
        var principal = CreatePrincipal(ClaimTypes.GivenName, "Jan");

        // Act
        var result = _tokenService.GetFirstNameFromToken(principal);

        // Assert
        result.Should().Be("Jan");
    }

    [Fact]
    public void GetLastNameFromToken_ShouldReturnCorrectLastName()
    {
        // Arrange
        var principal = CreatePrincipal(ClaimTypes.Surname, "Kowalski");

        // Act
        var result = _tokenService.GetLastNameFromToken(principal);

        // Assert
        result.Should().Be("Kowalski");
    }

    [Fact]
    public void GetUserRoleFromToken_ShouldReturnCorrectRole()
    {
        // Arrange
        var principal = CreatePrincipal(ClaimTypes.Role, "Adult");

        // Act
        var result = _tokenService.GetUserRoleFromToken(principal);

        // Assert
        result.Should().Be("Adult");
    }

    #region Helpers
    private ClaimsPrincipal CreatePrincipal(string claimType, string value)
    {
        var claims = new List<Claim> { new Claim(claimType, value) };
        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }

    private async Task<string> CreateTestToken(int userId)
    {
        var user = new AppUser { Id = userId, Email = "test@test.com" };
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        return await _tokenService.CreateAccessTokenAsync(user);
    }
    #endregion
}
