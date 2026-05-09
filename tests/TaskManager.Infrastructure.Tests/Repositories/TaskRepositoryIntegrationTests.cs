using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Tests.Fixtures;
using TaskManager.Infrastructure.Repositories;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Infrastructure.Tests.Repositories;

public class TaskRepositoryIntegrationTests : IClassFixture<PostgreSqlFixture>
{
    private readonly TaskRepository _repo;
    private readonly UserRepository _userRepo;

    public TaskRepositoryIntegrationTests(PostgreSqlFixture fixture)
    {
        _repo     = new TaskRepository(fixture.ConnectionFactory);
        _userRepo = new UserRepository(fixture.ConnectionFactory);
    }

    private static User CreateTestUser() =>
        User.Create($"Test {Guid.NewGuid():N}"[..20], $"{Guid.NewGuid():N}@test.com", "hash");

    private static TaskItem CreateTestTask(Guid userId) =>
        TaskItem.Create("Test task", "Description", TaskPriority.Medium, null, userId);

    [Fact]
    public async Task AddAsync_ValidTask_CanBeRetrievedById()
    {
        var user = CreateTestUser();
        await _userRepo.AddAsync(user);
        var task = CreateTestTask(user.Id);

        var id = await _repo.AddAsync(task);
        var retrieved = await _repo.GetByIdAsync(id, user.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(id);
        retrieved.Title.Should().Be("Test task");
        retrieved.Priority.Should().Be(TaskPriority.Medium);
    }

    [Fact]
    public async Task GetAllByUserAsync_ReturnsOnlyTasksForThatUser()
    {
        var userA = CreateTestUser();
        var userB = CreateTestUser();
        await _userRepo.AddAsync(userA);
        await _userRepo.AddAsync(userB);

        await _repo.AddAsync(CreateTestTask(userA.Id));
        await _repo.AddAsync(CreateTestTask(userA.Id));
        await _repo.AddAsync(CreateTestTask(userB.Id));

        var tasksA = await _repo.GetAllByUserAsync(userA.Id);
        var tasksB = await _repo.GetAllByUserAsync(userB.Id);

        tasksA.Should().HaveCount(2).And.OnlyContain(t => t.UserId == userA.Id);
        tasksB.Should().HaveCount(1).And.OnlyContain(t => t.UserId == userB.Id);
    }

    [Fact]
    public async Task UpdateAsync_ChangesFieldsInDatabase()
    {
        var user = CreateTestUser();
        await _userRepo.AddAsync(user);
        var task = CreateTestTask(user.Id);
        await _repo.AddAsync(task);

        var updated = TaskItem.Reconstitute(
            task.Id, "Updated title", "Updated desc",
            DomainTaskStatus.InProgress, TaskPriority.High,
            null, user.Id, task.CreatedAt, DateTime.UtcNow);

        await _repo.UpdateAsync(updated);
        var retrieved = await _repo.GetByIdAsync(task.Id, user.Id);

        retrieved!.Title.Should().Be("Updated title");
        retrieved.Priority.Should().Be(TaskPriority.High);
        retrieved.Status.Value.Should().Be(DomainTaskStatus.InProgress);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTaskFromDatabase()
    {
        var user = CreateTestUser();
        await _userRepo.AddAsync(user);
        var task = CreateTestTask(user.Id);
        await _repo.AddAsync(task);

        await _repo.DeleteAsync(task.Id, user.Id);
        var retrieved = await _repo.GetByIdAsync(task.Id, user.Id);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DoesNotReturnTaskOwnedByAnotherUser()
    {
        var owner = CreateTestUser();
        var other = CreateTestUser();
        await _userRepo.AddAsync(owner);
        await _userRepo.AddAsync(other);

        var task = CreateTestTask(owner.Id);
        await _repo.AddAsync(task);

        // other user tries to access owner's task
        var retrieved = await _repo.GetByIdAsync(task.Id, other.Id);

        retrieved.Should().BeNull();
    }
}
