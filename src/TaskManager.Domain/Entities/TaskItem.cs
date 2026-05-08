using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.ValueObjects;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskStatusValue Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string? description, TaskPriority priority,
        DateTime? dueDate, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");

        if (title.Length > 200)
            throw new DomainException("Title cannot exceed 200 characters.");

        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow.Date)
            throw new DomainException("Due date cannot be in the past.");

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TaskStatusValue.Pending(),
            Priority = priority,
            DueDate = dueDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static TaskItem Reconstitute(Guid id, string title, string? description,
        DomainTaskStatus status, TaskPriority priority, DateTime? dueDate,
        Guid userId, DateTime createdAt, DateTime updatedAt) => new()
    {
        Id = id,
        Title = title,
        Description = description,
        Status = TaskStatusValue.Create(status),
        Priority = priority,
        DueDate = dueDate,
        UserId = userId,
        CreatedAt = createdAt,
        UpdatedAt = updatedAt
    };

    public void UpdateDetails(string title, string? description, TaskPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");

        if (title.Length > 200)
            throw new DomainException("Title cannot exceed 200 characters.");

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(DomainTaskStatus newStatus)
    {
        var next = TaskStatusValue.Create(newStatus);

        if (!Status.CanTransitionTo(next))
            throw new DomainException(
                $"Cannot transition from '{Status}' to '{next}'.");

        Status = next;
        UpdatedAt = DateTime.UtcNow;
    }
}
