using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock = new();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _sut = new TaskService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTaskDtos()
    {
        var tasks = new List<TaskItem>
        {
            TaskItem.Create("Task 1", null, TaskPriority.Low, null, _userId),
            TaskItem.Create("Task 2", "Desc", TaskPriority.High, null, _userId)
        };

        _repoMock.Setup(r => r.GetAllByUserAsync(_userId, default))
            .ReturnsAsync(tasks);

        var result = await _sut.GetAllAsync(_userId);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Task 1");
        result[1].Title.Should().Be("Task 2");
    }

    [Fact]
    public async Task GetByIdAsync_TaskNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, default))
            .ReturnsAsync((TaskItem?)null);

        var act = async () => await _sut.GetByIdAsync(Guid.NewGuid(), _userId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedTaskDto()
    {
        var request = new CreateTaskRequest("New Task", "Desc", "High", null);
        var taskId = Guid.NewGuid();
        var createdTask = TaskItem.Create("New Task", "Desc", TaskPriority.High, null, _userId);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), default))
            .ReturnsAsync(taskId);

        _repoMock.Setup(r => r.GetByIdAsync(taskId, _userId, default))
            .ReturnsAsync(createdTask);

        var result = await _sut.CreateAsync(request, _userId);

        result.Title.Should().Be("New Task");
        result.Priority.Should().Be("High");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsValidationException()
    {
        var request = new CreateTaskRequest("", null, "Medium", null);

        var act = async () => await _sut.CreateAsync(request, _userId);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_TaskNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, default))
            .ReturnsAsync((TaskItem?)null);

        var act = async () => await _sut.DeleteAsync(Guid.NewGuid(), _userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ChangeStatusAsync_ValidTransition_ReturnsUpdatedTask()
    {
        var task = TaskItem.Create("Title", null, TaskPriority.Medium, null, _userId);
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, default))
            .ReturnsAsync(task);

        var result = await _sut.ChangeStatusAsync(task.Id, new ChangeStatusRequest("InProgress"), _userId);

        result.Status.Should().Be("InProgress");
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), default), Times.Once);
    }
}
