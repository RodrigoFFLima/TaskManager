using FluentAssertions;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.ValueObjects;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Tests.ValueObjects;

public class TaskStatusValueTests
{
    [Theory]
    [InlineData(DomainTaskStatus.Pending)]
    [InlineData(DomainTaskStatus.InProgress)]
    [InlineData(DomainTaskStatus.Completed)]
    [InlineData(DomainTaskStatus.Cancelled)]
    public void Create_ValidStatus_ReturnsInstance(DomainTaskStatus status)
    {
        var vo = TaskStatusValue.Create(status);
        vo.Value.Should().Be(status);
    }

    [Fact]
    public void Create_InvalidStatus_ThrowsDomainException()
    {
        var act = () => TaskStatusValue.Create((DomainTaskStatus)99);
        act.Should().Throw<DomainException>().WithMessage("*Invalid task status*");
    }

    [Theory]
    [InlineData(DomainTaskStatus.Pending,    DomainTaskStatus.InProgress, true)]
    [InlineData(DomainTaskStatus.Pending,    DomainTaskStatus.Cancelled,  true)]
    [InlineData(DomainTaskStatus.InProgress, DomainTaskStatus.Completed,  true)]
    [InlineData(DomainTaskStatus.InProgress, DomainTaskStatus.Cancelled,  true)]
    [InlineData(DomainTaskStatus.Pending,    DomainTaskStatus.Completed,  false)]
    [InlineData(DomainTaskStatus.Completed,  DomainTaskStatus.Pending,    false)]
    [InlineData(DomainTaskStatus.Cancelled,  DomainTaskStatus.Pending,    false)]
    [InlineData(DomainTaskStatus.Completed,  DomainTaskStatus.Cancelled,  false)]
    public void CanTransitionTo_ReturnsExpected(DomainTaskStatus from, DomainTaskStatus to, bool expected)
    {
        var fromVo = TaskStatusValue.Create(from);
        var toVo   = TaskStatusValue.Create(to);

        fromVo.CanTransitionTo(toVo).Should().Be(expected);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = TaskStatusValue.Pending();
        var b = TaskStatusValue.Pending();

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        TaskStatusValue.Pending().Equals(TaskStatusValue.InProgress()).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsStatusName()
    {
        TaskStatusValue.Completed().ToString().Should().Be("Completed");
    }

    [Fact]
    public void FactoryMethods_ReturnCorrectStatuses()
    {
        TaskStatusValue.Pending().Value.Should().Be(DomainTaskStatus.Pending);
        TaskStatusValue.InProgress().Value.Should().Be(DomainTaskStatus.InProgress);
        TaskStatusValue.Completed().Value.Should().Be(DomainTaskStatus.Completed);
        TaskStatusValue.Cancelled().Value.Should().Be(DomainTaskStatus.Cancelled);
    }
}
