using Npgsql;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, name, email, password_hash, created_at, updated_at
            FROM users WHERE id = @id
            """;
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapUser(reader) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, name, email, password_hash, created_at, updated_at
            FROM users WHERE email = @email
            """;
        cmd.Parameters.AddWithValue("@email", email.ToLowerInvariant());

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapUser(reader) : null;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EXISTS(SELECT 1 FROM users WHERE email = @email)";
        cmd.Parameters.AddWithValue("@email", email.ToLowerInvariant());

        var result = await cmd.ExecuteScalarAsync(ct);
        return (bool)result!;
    }

    public async Task<Guid> AddAsync(User user, CancellationToken ct = default)
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO users (id, name, email, password_hash, created_at, updated_at)
            VALUES (@id, @name, @email, @passwordHash, @createdAt, @updatedAt)
            RETURNING id
            """;
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@name", user.Name);
        cmd.Parameters.AddWithValue("@email", user.Email.Value);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt);
        cmd.Parameters.AddWithValue("@updatedAt", user.UpdatedAt);

        var result = await cmd.ExecuteScalarAsync(ct);
        return (Guid)result!;
    }

    private static User MapUser(NpgsqlDataReader r) => User.Reconstitute(
        id: r.GetGuid(0),
        name: r.GetString(1),
        email: r.GetString(2),
        passwordHash: r.GetString(3),
        createdAt: r.GetDateTime(4),
        updatedAt: r.GetDateTime(5)
    );
}
