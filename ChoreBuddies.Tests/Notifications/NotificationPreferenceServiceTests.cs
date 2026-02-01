using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Notifications.NotificationPreferences;
using Moq;
using Shared.Notifications;

namespace ChoreBuddies.Tests.Notifications;

public class NotificationPreferenceServiceTests
{
    private readonly Mock<INotificationPreferenceRepository> _repoMock;
    private readonly NotificationPreferenceService _service;
    public NotificationPreferenceServiceTests()
    {
        _repoMock = new Mock<INotificationPreferenceRepository>();
        _service = new NotificationPreferenceService(_repoMock.Object);
    }

    [Fact]
    public async Task GetActiveChannelsAsync_ReturnsEmptyList_WhenUserIsNull()
    {
        var result = await _service.GetActiveChannelsAsync(null, NotificationEvent.NewChore);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveChannelsAsync_ReturnsEnabledChannels_FromUserPreferences()
    {
        var user = new AppUser();
        user.NotificationPreferences = new List<NotificationPreference>
            {
                new() { User=user, Type = NotificationEvent.NewChore, Channel = NotificationChannel.Email, IsEnabled = true },
                new() { User=user, Type = NotificationEvent.NewChore, Channel = NotificationChannel.Push, IsEnabled = false }
            };

        var result = await _service.GetActiveChannelsAsync(user, NotificationEvent.NewChore);
        Assert.Single(result);
        Assert.Contains(NotificationChannel.Email, result);
    }
    [Fact]
    public async Task GetActiveChannelsAsync_ReturnsDefaultEmail_WhenNoPreferencesExist()
    {
        var user = new AppUser { Id = 1 };
        _repoMock.Setup(r => r.GetEnabledChannelTypesAsync(user.Id, NotificationEvent.NewChore, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<NotificationChannel>());
        _repoMock.Setup(r => r.HasAnyPreferenceForEventAsync(user.Id, NotificationEvent.NewChore, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        var result = await _service.GetActiveChannelsAsync(user, NotificationEvent.NewChore);
        Assert.Single(result);
        Assert.Contains(NotificationChannel.Email, result);
    }
    [Fact]
    public async Task CreateDefaultPreferencesAsync_AddsEmailEnabledAndPushDisabled()
    {
        var user = new AppUser { Id = 1 };

        await _service.CreateDefaultPreferencesAsync(user);

        _repoMock.Verify(r => r.AddPreferencesAsync(It.Is<List<NotificationPreference>>(prefs =>
            prefs.Count == Enum.GetValues(typeof(NotificationEvent)).Length * 2 &&
            prefs.Any(p => p.Channel == NotificationChannel.Email && p.IsEnabled) &&
            prefs.Any(p => p.Channel == NotificationChannel.Push && !p.IsEnabled)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllUserConfigAsync_ReturnsEmpty_WhenUserIsNull()
    {
        var result = await _service.GetAllUserConfigAsync(null);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllUserConfigAsync_ReturnsUserPreferences_WhenAvailable()
    {
        var user = new AppUser();
        user.NotificationPreferences = new List<NotificationPreference>
            {
                new() { User=user, Type = NotificationEvent.NewChore, Channel = NotificationChannel.Email, IsEnabled = true },
                new() { User=user, Type = NotificationEvent.NewChore, Channel = NotificationChannel.Push, IsEnabled = false }
            };

        var result = await _service.GetAllUserConfigAsync(user);
        Assert.Equal(2, result.Count);
    }
    [Fact]
    public async Task GetAllUserConfigAsync_FetchesFromRepository_WhenUserPreferencesNull()
    {
        var user = new AppUser { Id = 1 };
        var repoPrefs = new List<NotificationPreference>
        {
            new() {User=user, Type = NotificationEvent.NewChore, Channel = NotificationChannel.Email, IsEnabled = true }
        };
        _repoMock.Setup(r => r.GetAllUserPreferencesAsync(user.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(repoPrefs);

        var result = await _service.GetAllUserConfigAsync(user);
        Assert.Single(result);
        Assert.Equal(NotificationChannel.Email, result.First().Channel);
    }
    [Fact]
    public async Task UpdatePreferenceAsync_DoesNothing_WhenUserOrDtoIsNull()
    {
        await _service.UpdatePreferenceAsync(null, new NotificationPreferenceDto());
        await _service.UpdatePreferenceAsync(new AppUser(), null);

        _repoMock.Verify(r => r.UpdatePreferenceAsync(It.IsAny<NotificationPreference>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task UpdatePreferenceAsync_DoesNothing_WhenExistingPreferenceIsNull()
    {
        var user = new AppUser { Id = 1 };
        var dto = new NotificationPreferenceDto { Type = NotificationEvent.NewChore, Channel = NotificationChannel.Email, IsEnabled = false };

        _repoMock.Setup(r => r.GetPreferenceByKeysAsync(user.Id, dto.Type, dto.Channel, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((NotificationPreference?)null);

        await _service.UpdatePreferenceAsync(user, dto);

        _repoMock.Verify(r => r.UpdatePreferenceAsync(It.IsAny<NotificationPreference>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePreferenceAsync_UpdatesPreference_WhenExists()
    {
        var user = new AppUser { Id = 1 };
        var dto = new NotificationPreferenceDto { Type = NotificationEvent.NewChore, Channel = NotificationChannel.Email, IsEnabled = false };
        var existingPreference = new NotificationPreference { User = user, Type = dto.Type, Channel = dto.Channel, IsEnabled = true };

        _repoMock.Setup(r => r.GetPreferenceByKeysAsync(user.Id, dto.Type, dto.Channel, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(existingPreference);

        await _service.UpdatePreferenceAsync(user, dto);

        _repoMock.Verify(r => r.UpdatePreferenceAsync(It.Is<NotificationPreference>(p => p.IsEnabled == dto.IsEnabled), It.IsAny<CancellationToken>()), Times.Once);
    }
}
