using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.Email;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shared.Authentication;

namespace ChoreBuddies.Tests.Authentication;

public class AuthServiceTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationPreferenceService> _notificationServiceMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificationServiceMock = new Mock<INotificationPreferenceService>();
        _timeProviderMock = new Mock<TimeProvider>();

        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _authService = new AuthService(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            _timeProviderMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object
        );
    }

    // --- LOGIN ---

    [Fact]
    public async Task LoginUserAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var request = new LoginRequestDto("test@test.com", "Password123!");
        var user = new AppUser { Email = request.Email, Id = 1 };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.CreateAccessTokenAsync(user))
            .ReturnsAsync("valid_access_token");
        _tokenServiceMock.Setup(x => x.CreateRefreshToken())
            .Returns("valid_refresh_token");

        // Act
        var result = await _authService.LoginUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("valid_access_token");
        result.RefreshToken.Should().Be("valid_refresh_token");
    }

    [Fact]
    public async Task LoginUserAsync_WithInvalidPassword_ThrowsLoginFailedException()
    {
        // Arrange
        var request = new LoginRequestDto("test@test.com", "WrongPass");
        var user = new AppUser { Email = request.Email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<LoginFailedException>(() => _authService.LoginUserAsync(request));
    }

    [Fact]
    public async Task LoginUserAsync_WithNonExistingUser_ThrowsLoginFailedException()
    {
        // Arrange
        var request = new LoginRequestDto("ghost@test.com", "Pass");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<LoginFailedException>(() => _authService.LoginUserAsync(request));
    }

    // --- SIGNUP ---

    [Fact]
    public async Task SignupUserAsync_WhenEmailExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var request = new RegisterRequestDto(
            "existing@test.com",
            "Pass123!",
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-25),
            "User"
        );
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new AppUser());

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _authService.SignupUserAsync(request));
    }

    [Fact]
    public async Task SignupUserAsync_Success_CreatesUserAndSendsEmail()
    {
        // Arrange
        var request = new RegisterRequestDto(
            "new@test.com",
            "Pass123!",
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-25),
            "NewUser"
        );

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock.Setup(x => x.CreateAccessTokenAsync(It.IsAny<AppUser>()))
            .ReturnsAsync("access_token");
        _tokenServiceMock.Setup(x => x.CreateRefreshToken())
            .Returns("refresh_token");
        _tokenServiceMock.Setup(x => x.GetRefreshTokenExpiration())
            .Returns(DateTime.UtcNow.AddDays(7));

        // Act
        var result = await _authService.SignupUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");

        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Once);
    }

    [Fact]
    public async Task SignupUserAsync_IdentityFailure_ThrowsRegistrationFailedException()
    {
        // Arrange
        var request = new RegisterRequestDto(
            "weak@test.com",
            "123",
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-20),
            "User"
        );
        var identityErrors = new IdentityError[] { new IdentityError { Description = "Password too short" } };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RegistrationFailedException>(() => _authService.SignupUserAsync(request));
        ex.Message.Should().Contain("Password too short");
    }

    // --- REFRESH TOKEN / REVOKE ---

    [Fact]
    public async Task RevokeRefreshTokenAsync_UserExists_ClearsTokenAndReturnsTrue()
    {
        // Arrange
        int userId = 10;
        var user = new AppUser { Id = userId, RefreshToken = "old_token" };
        var now = DateTimeOffset.UtcNow;

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RevokeRefreshTokenAsync(userId);

        // Assert
        result.Should().BeTrue();
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiry.Should().Be(now.DateTime);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        int userId = 99;
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _authService.RevokeRefreshTokenAsync(userId);

        // Assert
        result.Should().BeFalse();
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
    }

    // --- GENERATE ACCESS TOKEN ---

    [Fact]
    public async Task GenerateAccessTokenAsync_UserExists_ReturnsToken()
    {
        // Arrange
        int userId = 5;
        var user = new AppUser { Id = userId };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.CreateAccessTokenAsync(user))
            .ReturnsAsync("new_access_token");

        // Act
        var result = await _authService.GenerateAccessTokenAsync(userId);

        // Assert
        result.Should().Be("new_access_token");
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        int userId = 99;
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((AppUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.GenerateAccessTokenAsync(userId));
    }
}
