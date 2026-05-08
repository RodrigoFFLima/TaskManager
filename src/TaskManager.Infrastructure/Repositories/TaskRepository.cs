using Npgsql;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;
using DomainTaskPriority = TaskManager.Domain.Enums.TaskPriority;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TaskRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, title, description, status, priority, due_date, user_id, created_at, updated_at
            FROM tasks
            WHERE id = @id AND user_id = @userId
            """;
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapTask(reader) : null;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, title, description, status, priority, due_date, user_id, created_at, updated_at
            FROM tasks
            WHERE user_id = @userId
            ORDER BY created_at DESC
            """;
        cmd.Parameters.AddWithValue("@userId", userId);

        var tasks = new List<TaskItem>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            tasks.Add(MapTask(reader));

        return tasks;
    }

    public async Task<Guid> AddAsync(TaskItem task, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO tasks (id, title, description, status, priority, due_date, user_id, created_at, updated_at)
            VALUES (@id, @title, @description, @status, @priority, @dueDate, @userId, @createdAt, @updatedAt)
            RETURNING id
            """;
        cmd.Parameters.AddWithValue("@id", task.Id);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@description", task.Description as object ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", (short)task.Status.Value);
        cmd.Parameters.AddWithValue("@priority", (short)task.Priority);
        cmd.Parameters.AddWithValue("@dueDate", task.DueDate as object ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@userId", task.UserId);
        cmd.Parameters.AddWithValue("@createdAt", task.CreatedAt);
        cmd.Parameters.AddWithValue("@updatedAt", task.UpdatedAt);

        var result = await cmd.ExecuteScalarAsync(ct);
        return (Guid)result!;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE tasks
            SET title = @title,
                description = @description,
                status = @status,
                priority = @priority,
                due_date = @dueDate,
                updated_at = @updatedAt
            WHERE id = @id AND user_id = @userId
            """;
        cmd.Parameters.AddWithValue("@id", task.Id);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@description", task.Description as object ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", (short)task.Status.Value);
        cmd.Parameters.AddWithValue("@priority", (short)task.Priority);
        cmd.Parameters.AddWithValue("@dueDate", task.DueDate as object ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@updatedAt", task.UpdatedAt);
        cmd.Parameters.AddWithValue("@userId", task.UserId);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM tasks WHERE id = @id AND user_id = @userId";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", userId);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static TaskItem MapTask(NpgsqlDataReader r) => TaskItem.Reconstitute(
        id: r.GetGuid(0),
        title: r.GetString(1),
        description: r.IsDBNull(2) ? null : r.GetString(2),
        status: (DomainTaskStatus)r.GetInt16(3),
        priority: (DomainTaskPriority)r.GetInt16(4),
        dueDate: r.IsDBNull(5) ? null : r.GetDateTime(5),
        userId: r.GetGuid(6),
        createdAt: r.GetDateTime(7),
        updatedAt: r.GetDateTime(8)
    );
}
