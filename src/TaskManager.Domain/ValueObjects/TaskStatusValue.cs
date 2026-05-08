using TaskManager.Domain.Exceptions;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.ValueObjects;

public sealed class TaskStatusValue : IEquatable<TaskStatusValue>
{
    public DomainTaskStatus Value { get; }

    private TaskStatusValue(DomainTaskStatus value) => Value = value;

    public static TaskStatusValue Create(DomainTaskStatus status)
    {
        if (!Enum.IsDefined(typeof(DomainTaskStatus), status))
            throw new DomainException($"Invalid task status: {status}.");

        return new TaskStatusValue(status);
    }

    public static TaskStatusValue Pending() => new(DomainTaskStatus.Pending);
    public static TaskStatusValue InProgress() => new(DomainTaskStatus.InProgress);
    public static TaskStatusValue Completed() => new(DomainTaskStatus.Completed);
    public static TaskStatusValue Cancelled() => new(DomainTaskStatus.Cancelled);

    public bool CanTransitionTo(TaskStatusValue next) => (Value, next.Value) switch
    {
        (DomainTaskStatus.Pending, DomainTaskStatus.InProgress) => true,
        (DomainTaskStatus.Pending, DomainTaskStatus.Cancelled) => true,
        (DomainTaskStatus.InProgress, DomainTaskStatus.Completed) => true,
        (DomainTaskStatus.InProgress, DomainTaskStatus.Cancelled) => true,
        _ => false
    };

    public bool Equals(TaskStatusValue? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is TaskStatusValue s && Equals(s);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
}
