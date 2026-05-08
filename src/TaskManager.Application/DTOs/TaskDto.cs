namespace TaskManager.Application.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate
);

public record ChangeStatusRequest(string Status);
