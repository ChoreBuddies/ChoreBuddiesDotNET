using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.Email;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using ChoreBuddies.Backend.Infrastructure.Authentication;
using ChoreBuddies.Backend.Infrastructure.Authentication.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shared.Authentication;

namespace ChoreBuddies.Backend.Tests.Features.Authentication;

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

        // Mockowanie UserManager wymaga przekazania parametrów do konstruktora (store, options, etc.)
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _authService = new AuthService(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            _timeProviderMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object
        );
    }

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
            .ReturnsAsync(false); // Błędne hasło

        // Act & Assert
        await Assert.ThrowsAsync<LoginFailedException>(() => _authService.LoginUserAsync(request));
    }

    [Fact]
    public async Task SignupUserAsync_WhenEmailExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var request = new RegisterRequestDto("existing@test.com", "Pass123!", "User");
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new AppUser()); // Użytkownik istnieje

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _authService.SignupUserAsync(request));
    }
}
