using FluentAssertions;
using FluentValidation;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Tests.Services;

public class TaskServiceEdgeCaseTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceEdgeCaseTests()
    {
        _sut = new TaskService(_repoMock.Object);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_TaskNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, default))
            .ReturnsAsync((TaskItem?)null);

        var act = async () =>
            await _sut.UpdateAsync(Guid.NewGuid(), new UpdateTaskRequest("T", null, "Low", null), _userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_InvalidPriority_ThrowsValidationException()
    {
        var act = async () =>
            await _sut.UpdateAsync(Guid.NewGuid(), new UpdateTaskRequest("T", null, "INVALID", null), _userId);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidData_CallsRepositoryUpdate()
    {
        var task = TaskItem.Create("Old", null, TaskPriority.Low, null, _userId);

        _repoMock.Setup(r => r.GetByIdAsync(task.Id, _userId, default)).ReturnsAsync(task);

        var result = await _sut.UpdateAsync(task.Id, new UpdateTaskRequest("New", "Desc", "High", null), _userId);

        result.Title.Should().Be("New");
        result.Priority.Should().Be("High");
        _repoMock.Verify(r => r.UpdateAsync(task, default), Times.Once);
    }

    // ── ChangeStatusAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeStatusAsync_InvalidStatusString_ThrowsDomainException()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Low, null, _userId);
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, _userId, default)).ReturnsAsync(task);

        var act = async () =>
            await _sut.ChangeStatusAsync(task.Id, new ChangeStatusRequest("NotAStatus"), _userId);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Invalid status*");
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_ThrowsDomainException()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Medium, null, _userId);
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, _userId, default)).ReturnsAsync(task);

        // Pending → Completed is not allowed
        var act = async () =>
            await _sut.ChangeStatusAsync(task.Id, new ChangeStatusRequest("Completed"), _userId);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*transition*");
    }

    [Fact]
    public async Task ChangeStatusAsync_TaskNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, default))
            .ReturnsAsync((TaskItem?)null);

        var act = async () =>
            await _sut.ChangeStatusAsync(Guid.NewGuid(), new ChangeStatusRequest("InProgress"), _userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_EmptyList_ReturnsEmptyCollection()
    {
        _repoMock.Setup(r => r.GetAllByUserAsync(_userId, default))
            .ReturnsAsync(new List<TaskItem>());

        var result = await _sut.GetAllAsync(_userId);

        result.Should().BeEmpty();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_TaskExists_CallsRepositoryDelete()
    {
        var task = TaskItem.Create("T", null, TaskPriority.Low, null, _userId);
        _repoMock.Setup(r => r.GetByIdAsync(task.Id, _userId, default)).ReturnsAsync(task);

        await _sut.DeleteAsync(task.Id, _userId);

        _repoMock.Verify(r => r.DeleteAsync(task.Id, _userId, default), Times.Once);
    }

    // ── CreateAsync — DTO mapping ────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_MapsAllFieldsCorrectly()
    {
        var taskId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(5);
        var created = TaskItem.Create("My Task", "My Desc", TaskPriority.Critical, dueDate, _userId);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), default)).ReturnsAsync(taskId);
        _repoMock.Setup(r => r.GetByIdAsync(taskId, _userId, default)).ReturnsAsync(created);

        var result = await _sut.CreateAsync(
            new CreateTaskRequest("My Task", "My Desc", "Critical", dueDate), _userId);

        result.Title.Should().Be("My Task");
        result.Description.Should().Be("My Desc");
        result.Priority.Should().Be("Critical");
        result.Status.Should().Be("Pending");
        result.UserId.Should().Be(_userId);
    }
}
