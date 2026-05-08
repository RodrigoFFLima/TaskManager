using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken ct = default);
    Task<TaskDto> ChangeStatusAsync(Guid id, ChangeStatusRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
