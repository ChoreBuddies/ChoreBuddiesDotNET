using AutoMapper;
using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Features.Chores;
using ChoreBuddies.Backend.Features.Notifications;
using ChoreBuddies.Backend.Features.Users;
using FluentAssertions;
using Moq;
using Shared.Chores;

namespace ChoreBuddies.Tests.Chores;

public class ChoresServiceTests
{
    private readonly Mock<IChoresRepository> _repo;
    private readonly Mock<IAppUserService> _userService;
    private readonly Mock<INotificationService> _notificationService;
    private readonly Mock<IMapper> _mapper;
    private readonly ChoresService _service;

    public ChoresServiceTests()
    {
        _repo = new Mock<IChoresRepository>();
        _userService = new Mock<IAppUserService>();
        _mapper = new Mock<IMapper>();
        _notificationService = new Mock<INotificationService>();

        _service = new ChoresService(_mapper.Object, _repo.Object, _userService.Object, _notificationService.Object);
    }

    // ---------------------------
    // GetChoreDetailsAsync
    // ---------------------------
    [Fact]
    public async Task GetChoreDetailsAsync_ShouldReturnMappedChore()
    {
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 5 };
        var dto = new ChoreDto(5, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _repo.Setup(x => x.GetChoreByIdAsync(5)).ReturnsAsync(chore);
        _mapper.Setup(x => x.Map<ChoreDto>(chore)).Returns(dto);

        var result = await _service.GetChoreDetailsAsync(5);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetChoreDetailsAsync_ShouldThrow_WhenNotFound()
    {
        _repo.Setup(x => x.GetChoreByIdAsync(5)).ReturnsAsync((Chore?)null);

        await _service.Invoking(s => s.GetChoreDetailsAsync(5))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore not found");
    }

    // ---------------------------
    // CreateChoreAsync
    // ---------------------------
    [Fact]
    public async Task CreateChoreAsync_ShouldMapCreateDtoAndReturnCreatedDtoAndNotSendNotification_WhenUserIdNull()
    {
        var createDto = new CreateChoreDto("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10);
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 1 };
        var choreDto = new ChoreDto(1, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _mapper.Setup(m => m.Map<Chore>(createDto)).Returns(chore);
        _repo.Setup(r => r.CreateChoreAsync(chore)).ReturnsAsync(chore);
        _mapper.Setup(m => m.Map<ChoreDto>(chore)).Returns(choreDto);

        var result = await _service.CreateChoreAsync(createDto);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);

        _notificationService.Verify(n => n.SendNewChoreNotificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<DateTime>(), default),
            Times.Never);
    }
    [Fact]
    public async Task CreateChoreAsync_ShouldMapCreateDtoAndReturnCreatedDtoAndSendNotification_WhenUserIdNotNull()
    {
        var createDto = new CreateChoreDto("test", "testt", 2, 1, DateTime.Now, Status.Unassigned, "kitchen", 10);
        var chore = new Chore("test", "testt", 2, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 1 };
        var choreDto = new ChoreDto(1, "test", "testt", 2, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _mapper.Setup(m => m.Map<Chore>(createDto)).Returns(chore);
        _repo.Setup(r => r.CreateChoreAsync(chore)).ReturnsAsync(chore);
        _mapper.Setup(m => m.Map<ChoreDto>(chore)).Returns(choreDto);

        var result = await _service.CreateChoreAsync(createDto);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);

        _notificationService.Verify(n => n.SendNewChoreNotificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<DateTime>(), default),
            Times.Once);
    }

    // ---------------------------
    // UpdateChoreAsync
    // ---------------------------
    [Fact]
    public async Task UpdateChoreAsync_ShouldUpdateAndReturnChoreDtoAndNotSendNotification_WhenUserIdDidNotChange()
    {
        var dto = new ChoreDto(2, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);
        var mappedChore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 2 };
        var updatedChore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 2 };
        var updatedDto = new ChoreDto(2, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _repo.Setup(x => x.GetChoreByIdAsync(2)).ReturnsAsync(mappedChore);

        _mapper.Setup(x => x.Map<Chore>(dto)).Returns(mappedChore);

        _repo.Setup(x => x.UpdateChoreAsync(mappedChore)).ReturnsAsync(updatedChore);

        _mapper.Setup(x => x.Map<ChoreDto>(updatedChore)).Returns(updatedDto);

        var result = await _service.UpdateChoreAsync(dto);

        result!.Id.Should().Be(2);

        _notificationService.Verify(n => n.SendNewChoreNotificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<DateTime>(), default),
            Times.Never);
    }
    [Fact]
    public async Task UpdateChoreAsync_ShouldUpdateAndReturnChoreDtoAndSendNotification_WhenUserIdIsNull()
    {
        var dto = new ChoreDto(2, "test", "testt", 3, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);
        var mappedChore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 2 };
        var updatedChore = new Chore("test", "testt", 3, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 2 };
        var updatedDto = new ChoreDto(2, "test", "testt", 3, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _repo.Setup(x => x.GetChoreByIdAsync(2)).ReturnsAsync(mappedChore);

        _mapper.Setup(x => x.Map<Chore>(dto)).Returns(mappedChore);

        _repo.Setup(x => x.UpdateChoreAsync(mappedChore)).ReturnsAsync(updatedChore);

        _mapper.Setup(x => x.Map<ChoreDto>(updatedChore)).Returns(updatedDto);

        var result = await _service.UpdateChoreAsync(dto);

        result!.Id.Should().Be(2);

        _notificationService.Verify(n => n.SendNewChoreNotificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<DateTime>(), default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateChoreAsync_ShouldThrow_WhenChoreNotFound()
    {
        _repo.Setup(x => x.GetChoreByIdAsync(2)).ReturnsAsync((Chore?)null);

        await _service.Invoking(s => s.UpdateChoreAsync(new ChoreDto(2, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null)))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore not found");
    }

    // ---------------------------
    // DeleteChoreAsync
    // ---------------------------
    [Fact]
    public async Task DeleteChoreAsync_ShouldDeleteAndReturnChoreDto()
    {
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 3 };
        var choreDto = new ChoreDto(3, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _repo.Setup(r => r.GetChoreByIdAsync(3)).ReturnsAsync(chore);
        _repo.Setup(r => r.DeleteChoreAsync(chore)).ReturnsAsync(chore);
        _mapper.Setup(m => m.Map<ChoreDto>(chore)).Returns(choreDto);

        var result = await _service.DeleteChoreAsync(3);

        result!.Id.Should().Be(3);
    }

    [Fact]
    public async Task DeleteChoreAsync_ShouldThrow_WhenNotFound()
    {
        _repo.Setup(r => r.GetChoreByIdAsync(3)).ReturnsAsync((Chore?)null);

        await _service.Invoking(s => s.DeleteChoreAsync(3))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore not found");
    }

    // ---------------------------
    // GetUsersChoreDetailsAsync
    // ---------------------------
    [Fact]
    public async Task GetUsersChoreDetailsAsync_ShouldMapList()
    {
        var chores = new List<Chore> { new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 1 } };
        var dtos = new List<ChoreDto> { new ChoreDto(1, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) };

        _repo.Setup(r => r.GetUsersChoresAsync(10)).ReturnsAsync(chores);
        _mapper.Setup(m => m.Map<List<ChoreDto>>(chores)).Returns(dtos);

        var result = await _service.GetUsersChoreDetailsAsync(10);

        result.Should().HaveCount(1);
    }

    // ---------------------------
    // GetMyHouseholdChoreDetailsAsync
    // ---------------------------

    [Fact]
    public async Task GetMyHouseholdChoreDetailsAsync_ShouldMapList()
    {
        var chores = new List<Chore> { new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 7 } };
        var dtos = new List<ChoreDto> { new ChoreDto(7, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) };

        _repo.Setup(r => r.GetHouseholdChoresAsync(10)).ReturnsAsync(chores);
        _mapper.Setup(m => m.Map<List<ChoreDto>>(chores)).Returns(dtos);

        var result = await _service.GetMyHouseholdChoreDetailsAsync(10);

        result.Should().ContainSingle(x => x.Id == 7);
    }

    // ---------------------------
    // GetMyHouseholdUnverifiedChoresAsync
    // ---------------------------

    [Fact]
    public async Task GetMyHouseholdUnverifiedChoresAsync_ShouldMapList()
    {
        var chores = new List<Chore> { new Chore("test", "testt", 2, 1, DateTime.Now.AddDays(-1), Status.UnverifiedCompleted, "kitchen", 10, DateTime.Now) { Id = 7 } };
        var dtos = new List<ChoreOverviewDto> { new ChoreOverviewDto(7, "test", 2, Status.UnverifiedCompleted, "kitchen", DateTime.Now) };

        _repo.Setup(r => r.GetHouseholdUnverifiedChoresAsync(10)).ReturnsAsync(chores);
        _mapper.Setup(m => m.Map<List<ChoreOverviewDto>>(chores)).Returns(dtos);

        var result = await _service.GetMyHouseholdUnverifiedChoresAsync(10);

        result.Should().ContainSingle(x => x.Id == 7);
    }

    // ---------------------------
    // CreateChoreListAsync
    // ---------------------------
    [Fact]
    public async Task CreateChoreListAsync_ShouldMapAndReturnList()
    {
        var createDtos = new List<CreateChoreDto> { new CreateChoreDto("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10) };
        var chores = new List<Chore> { new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 1 } };
        var returnedChores = new List<Chore> { new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) };
        var resultDtos = new List<ChoreDto> { new ChoreDto(1, "test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) };

        _mapper.Setup(m => m.Map<List<Chore>>(createDtos)).Returns(chores);
        _repo.Setup(r => r.CreateChoreListAsync(chores)).ReturnsAsync(returnedChores);
        _mapper.Setup(m => m.Map<List<ChoreDto>>(returnedChores)).Returns(resultDtos);

        var result = await _service.CreateChoreListAsync(createDtos);

        result.Should().HaveCount(1);
    }

    // ---------------------------
    // AssignChoreAsync
    // ---------------------------

    [Fact]
    public async Task AssignChoreAsync_ShouldThrow_WhenChoreNotFound()
    {
        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync((Chore?)null);
        _userService.Setup(u => u.GetUserByIdAsync(10)).ReturnsAsync(new AppUser() { Id = 10 });

        await _service.Invoking(s => s.AssignChoreAsync(1, 10))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore not found");
    }
    [Fact]
    public async Task AssignChoreAsync_ShouldThrow_WhenUserNotFound()
    {
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Assigned, "kitchen", 10, null) { Id = 1 };
        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);
        _userService.Setup(u => u.GetUserByIdAsync(10)).ReturnsAsync((AppUser?)null);

        await _service.Invoking(s => s.AssignChoreAsync(1, 10))
            .Should().ThrowAsync<Exception>()
            .WithMessage("User not found");
    }
    [Fact]
    public async Task AssignChoreAsync_ShouldAssignChoreAndSendNotification()
    {
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null) { Id = 1 };
        var newChore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Assigned, "kitchen", 10, null) { Id = 1 };
        var user = new AppUser() { Id = 10 };
        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);
        _userService.Setup(u => u.GetUserByIdAsync(10)).ReturnsAsync(user);
        _repo.Setup(r => r.AssignChoreAsync(1, 10)).ReturnsAsync(newChore);

        await _service.AssignChoreAsync(1, 10);

        _repo.Verify(x => x.AssignChoreAsync(chore.Id, user.Id), Times.Once);

        _notificationService.Verify(n => n.SendNewChoreNotificationAsync(10, 1, It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<DateTime>(), default),
            Times.Once);
    }

    // ---------------------------
    // MarkChoreAsDoneAsync
    // ---------------------------
    [Fact]
    public async Task MarkChoreAsDoneAsync_ShouldMarkCompletedAndIncreaseUserPoints()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Assigned, "kitchen", 10, null) { Id = 1 };

        var user = new AppUser { Id = 10, PointsCount = 0 };
        var updatedChore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Completed, "kitchen", 10, DateTime.Now.AddDays(1)) { Id = 1 };
        var mappedDto = new ChoreDto(1, "test", "testt", 10, 1, DateTime.Now, Status.Completed, "kitchen", 10, DateTime.Now.AddDays(1));

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);
        _userService.Setup(u => u.GetUserByIdAsync(10)).ReturnsAsync(user);
        _userService.Setup(u => u.AddPointsToUser(10, 10)).ReturnsAsync(true);
        _repo.Setup(r => r.UpdateChoreAsync(chore)).ReturnsAsync(updatedChore);
        _mapper.Setup(m => m.Map<ChoreDto>(updatedChore)).Returns(mappedDto);

        var result = await _service.MarkChoreAsDoneAsync(1, 10, true);

        result.Status.Should().Be(Status.Completed);
    }
    [Fact]
    public async Task MarkChoreAsDoneAsync_ShouldMarkUnverifiedCompletedAndPointsNotAdded_WhenNotVerified()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Assigned, "kitchen", 10, null) { Id = 1 };

        var user = new AppUser { Id = 10, PointsCount = 0 };
        var updatedChore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.UnverifiedCompleted, "kitchen", 10, DateTime.Now.AddDays(1)) { Id = 1 };
        var mappedDto = new ChoreDto(1, "test", "testt", 10, 1, DateTime.Now, Status.UnverifiedCompleted, "kitchen", 10, DateTime.Now.AddDays(1));

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);
        _userService.Setup(u => u.GetUserByIdAsync(10)).ReturnsAsync(user);
        _repo.Setup(r => r.UpdateChoreAsync(chore)).ReturnsAsync(updatedChore);
        _mapper.Setup(m => m.Map<ChoreDto>(updatedChore)).Returns(mappedDto);

        var result = await _service.MarkChoreAsDoneAsync(1, 10, false);

        result.Status.Should().Be(Status.UnverifiedCompleted);
        _userService.Verify(u => u.AddPointsToUser(user.Id, chore.RewardPointsCount),
            Times.Never);
    }

    [Fact]
    public async Task MarkChoreAsDoneAsync_ShouldThrow_WhenChoreNotFound()
    {
        _repo.Setup(r => r.GetChoreByIdAsync(1))
            .ReturnsAsync((Chore?)null);

        await _service.Invoking(s => s.MarkChoreAsDoneAsync(1, 10, true))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore not found");
    }

    [Fact]
    public async Task MarkChoreAsDoneAsync_ShouldThrow_WhenChoreIsNotAssigned()
    {
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        await _service.Invoking(s => s.MarkChoreAsDoneAsync(1, 5, true))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore must be assigned in order to be able to mark as done");
    }

    [Fact]
    public async Task MarkChoreAsDoneAsync_ShouldThrow_WhenUserIsDifferent()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Assigned, "kitchen", 10, null) { Id = 1 };

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        await _service.Invoking(s => s.MarkChoreAsDoneAsync(1, 99, true))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Only user who has to do this chore can mark it as done");
    }
    [Fact]
    public async Task MarkChoreAsDoneAsync_ShouldThrow_WhenErrorWhileAddingPoints()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Assigned, "kitchen", 10, null) { Id = 1 };

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        _userService.Setup(u => u.AddPointsToUser(10, chore.RewardPointsCount)).ReturnsAsync(false);

        await _service.Invoking(s => s.MarkChoreAsDoneAsync(1, 10, true))
            .Should().ThrowAsync<Exception>()
            .WithMessage("There was an error while adding points to the user");
    }

    // ---------------------------
    // VerifyChoreAsync
    // ---------------------------

    [Fact]
    public async Task VerifyChoreAsync_ShouldThrow_WhenChoreNotFound()
    {
        _repo.Setup(r => r.GetChoreByIdAsync(1))
            .ReturnsAsync((Chore?)null);

        await _service.Invoking(s => s.VerifyChoreAsync(1, 10))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore not found");
    }

    [Fact]
    public async Task VerifyChoreAsync_ShouldThrow_WhenChoreStatusNotUnverifiedCompleted()
    {
        var chore = new Chore("test", "testt", null, 1, DateTime.Now, Status.Unassigned, "kitchen", 10, null);

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        await _service.Invoking(s => s.VerifyChoreAsync(1, 99))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Chore must be unverified completed in order to be able to verify");
    }
    [Fact]
    public async Task VerifyChoreAsync_ShouldThrow_WhenUserIsTheSame()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.UnverifiedCompleted, "kitchen", 10, DateTime.Now.AddDays(1)) { Id = 1 };

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        await _service.Invoking(s => s.VerifyChoreAsync(1, 10))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Only another user can verify the chore");
    }
    [Fact]
    public async Task VerifyChoreAsync_ShouldThrow_WhenErrorWhileAddingPoints()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.UnverifiedCompleted, "kitchen", 10, DateTime.Now.AddDays(1)) { Id = 1 };

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);

        _userService.Setup(u => u.AddPointsToUser(10, chore.RewardPointsCount)).ReturnsAsync(false);

        await _service.Invoking(s => s.VerifyChoreAsync(1, 5))
            .Should().ThrowAsync<Exception>()
            .WithMessage("There was an error while adding points to the user");
    }
    [Fact]
    public async Task VerifyChoreAsync_ShouldMarkCompletedAndIncreaseUserPoints()
    {
        var chore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.UnverifiedCompleted, "kitchen", 10, DateTime.Now.AddDays(1)) { Id = 1 };

        var user = new AppUser { Id = 10, PointsCount = 0 };
        var updatedChore = new Chore("test", "testt", 10, 1, DateTime.Now, Status.Completed, "kitchen", 10, DateTime.Now.AddDays(1)) { Id = 1 };
        var mappedDto = new ChoreDto(1, "test", "testt", 10, 1, DateTime.Now, Status.Completed, "kitchen", 10, DateTime.Now.AddDays(1));

        _repo.Setup(r => r.GetChoreByIdAsync(1)).ReturnsAsync(chore);
        _userService.Setup(u => u.GetUserByIdAsync(10)).ReturnsAsync(user);
        _userService.Setup(u => u.AddPointsToUser(10, 10)).ReturnsAsync(true);
        _repo.Setup(r => r.UpdateChoreAsync(chore)).ReturnsAsync(updatedChore);
        _mapper.Setup(m => m.Map<ChoreDto>(updatedChore)).Returns(mappedDto);

        var result = await _service.VerifyChoreAsync(1, 99);

        result.Status.Should().Be(Status.Completed);
    }
}

