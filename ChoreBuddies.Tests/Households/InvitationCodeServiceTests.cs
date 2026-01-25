using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Households;
using ChoreBuddies.Backend.Features.Households.Exceptions;
using FluentAssertions;
using Moq;

namespace ChoreBuddies.Tests.Households;
public class InvitationCodeServiceTests
{
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock;
    private readonly InvitationCodeService _sut;

    public InvitationCodeServiceTests()
    {
        _householdRepositoryMock = new Mock<IHouseholdRepository>();
        _sut = new InvitationCodeService(_householdRepositoryMock.Object);
    }

    [Fact]
    public async Task GenerateUniqueInvitationCodeAsync_ShouldReturnCode_WhenCodeDoesNotExist()
    {
        // Arrange
        _householdRepositoryMock
            .Setup(r => r.GetHouseholdByInvitationCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _sut.GenerateUniqueInvitationCodeAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Length.Should().Be(6);

        _householdRepositoryMock.Verify(
            r => r.GetHouseholdByInvitationCodeAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateUniqueInvitationCodeAsync_ShouldRetry_WhenCodeAlreadyExists()
    {
        // Arrange
        _householdRepositoryMock
            .SetupSequence(r => r.GetHouseholdByInvitationCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(new Household(0, "", "", ""))
            .ReturnsAsync((Household?)null);

        // Act
        var result = await _sut.GenerateUniqueInvitationCodeAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();

        _householdRepositoryMock.Verify(
            r => r.GetHouseholdByInvitationCodeAsync(It.IsAny<string>()),
            Times.Exactly(2));
    }
    [Fact]
    public async Task GenerateUniqueInvitationCodeAsync_ShouldThrow_WhenMaxAttemptsExceeded()
    {
        // Arrange
        _householdRepositoryMock
            .Setup(r => r.GetHouseholdByInvitationCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(new Household(0, "", "", ""));

        // Act
        var act = async () => await _sut.GenerateUniqueInvitationCodeAsync();

        // Assert
        await act.Should().ThrowAsync<InvitationCodeGenerationException>();

        _householdRepositoryMock.Verify(
            r => r.GetHouseholdByInvitationCodeAsync(It.IsAny<string>()),
            Times.Exactly(10));
    }
}
