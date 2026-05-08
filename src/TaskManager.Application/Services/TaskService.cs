using FluentValidation;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly CreateTaskValidator _createValidator;
    private readonly UpdateTaskValidator _updateValidator;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
        _createValidator = new CreateTaskValidator();
        _updateValidator = new UpdateTaskValidator();
    }

    public async Task<IReadOnlyList<TaskDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetAllByUserAsync(userId, ct);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<TaskDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId, ct)
            ?? throw new NotFoundException($"Task '{id}' not found.");

        return ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var priority = Enum.Parse<TaskPriority>(request.Priority);
        var task = TaskItem.Create(request.Title, request.Description, priority, request.DueDate, userId);

        var id = await _taskRepository.AddAsync(task, ct);
        var created = await _taskRepository.GetByIdAsync(id, userId, ct);
        return ToDto(created!);
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var task = await _taskRepository.GetByIdAsync(id, userId, ct)
            ?? throw new NotFoundException($"Task '{id}' not found.");

        var priority = Enum.Parse<TaskPriority>(request.Priority);
        task.UpdateDetails(request.Title, request.Description, priority, request.DueDate);

        await _taskRepository.UpdateAsync(task, ct);
        return ToDto(task);
    }

    public async Task<TaskDto> ChangeStatusAsync(Guid id, ChangeStatusRequest request, Guid userId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId, ct)
            ?? throw new NotFoundException($"Task '{id}' not found.");

        if (!Enum.TryParse<DomainTaskStatus>(request.Status, out var newStatus))
            throw new DomainException($"Invalid status: {request.Status}.");

        task.ChangeStatus(newStatus);
        await _taskRepository.UpdateAsync(task, ct);
        return ToDto(task);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId, ct)
            ?? throw new NotFoundException($"Task '{id}' not found.");

        await _taskRepository.DeleteAsync(task.Id, userId, ct);
    }

    private static TaskDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description,
        t.Status.Value.ToString(),
        t.Priority.ToString(),
        t.DueDate, t.UserId,
        t.CreatedAt, t.UpdatedAt
    );
}
