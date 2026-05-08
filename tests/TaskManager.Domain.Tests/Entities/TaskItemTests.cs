using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Tests.Entities;

public class TaskItemTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_ValidData_ReturnsTaskWithPendingStatus()
    {
        var task = TaskItem.Create("Write tests", "Test description", TaskPriority.High, null, UserId);

        task.Title.Should().Be("Write tests");
        task.Description.Should().Be("Test description");
        task.Status.Value.Should().Be(DomainTaskStatus.Pending);
        task.Priority.Should().Be(TaskPriority.High);
        task.UserId.Should().Be(UserId);
        task.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyTitle_ThrowsDomainException(string? title)
    {
        var act = () => TaskItem.Create(title!, null, TaskPriority.Medium, null, UserId);
        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void Create_TitleExceeds200Chars_ThrowsDomainException()
    {
        var title = new string('x', 201);
        var act = () => TaskItem.Create(title, null, TaskPriority.Low, null, UserId);
        act.Should().Throw<DomainException>().WithMessage("*200*");
    }

    [Fact]
    public void Create_PastDueDate_ThrowsDomainException()
    {
        var act = () => TaskItem.Create("Title", null, TaskPriority.Low, DateTime.UtcNow.AddDays(-1), UserId);
        act.Should().Throw<DomainException>().WithMessage("*past*");
    }

    [Fact]
    public void ChangeStatus_ValidTransition_UpdatesStatus()
    {
        var task = TaskItem.Create("Title", null, TaskPriority.Medium, null, UserId);

        task.ChangeStatus(DomainTaskStatus.InProgress);

        task.Status.Value.Should().Be(DomainTaskStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsDomainException()
    {
        var task = TaskItem.Create("Title", null, TaskPriority.Medium, null, UserId);

        var act = () => task.ChangeStatus(DomainTaskStatus.Completed);

        act.Should().Throw<DomainException>().WithMessage("*transition*");
    }

    [Fact]
    public void ChangeStatus_PendingToCancelled_Allowed()
    {
        var task = TaskItem.Create("Title", null, TaskPriority.Low, null, UserId);

        task.ChangeStatus(DomainTaskStatus.Cancelled);

        task.Status.Value.Should().Be(DomainTaskStatus.Cancelled);
    }

    [Fact]
    public void UpdateDetails_ValidData_UpdatesProperties()
    {
        var task = TaskItem.Create("Old Title", "Old desc", TaskPriority.Low, null, UserId);

        task.UpdateDetails("New Title", "New desc", TaskPriority.Critical, null);

        task.Title.Should().Be("New Title");
        task.Description.Should().Be("New desc");
        task.Priority.Should().Be(TaskPriority.Critical);
    }

    [Fact]
    public void Create_TrimsTitle_WhenTitleHasWhitespace()
    {
        var task = TaskItem.Create("  Trimmed  ", null, TaskPriority.Low, null, UserId);

        task.Title.Should().Be("Trimmed");
    }
}
