using Testcontainers.PostgreSql;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Tests.Fixtures;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("taskmanager_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public IDbConnectionFactory ConnectionFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionFactory = new DbConnectionFactory(_container.GetConnectionString());
        await RunMigrationsAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    private async Task RunMigrationsAsync()
    {
        await using var conn = ConnectionFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS users (
                id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name          VARCHAR(100)  NOT NULL,
                email         VARCHAR(254)  NOT NULL UNIQUE,
                password_hash TEXT          NOT NULL,
                created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
                updated_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS tasks (
                id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                title       VARCHAR(200)  NOT NULL,
                description TEXT,
                status      SMALLINT      NOT NULL DEFAULT 0,
                priority    SMALLINT      NOT NULL DEFAULT 1,
                due_date    TIMESTAMPTZ,
                user_id     UUID          NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                created_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
                updated_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW()
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }
}
