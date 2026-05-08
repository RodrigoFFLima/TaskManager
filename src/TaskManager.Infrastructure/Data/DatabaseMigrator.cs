using Npgsql;

namespace TaskManager.Infrastructure.Data;

public class DatabaseMigrator : IDatabaseMigrator
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseMigrator(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task MigrateAsync()
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = GetMigrationSql();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SeedAsync()
    {
        await using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = GetSeedSql();
        await cmd.ExecuteNonQueryAsync();
    }

    private static string GetMigrationSql() => """
        CREATE TABLE IF NOT EXISTS users (
            id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            name        VARCHAR(100)  NOT NULL,
            email       VARCHAR(254)  NOT NULL UNIQUE,
            password_hash TEXT        NOT NULL,
            created_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
            updated_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW()
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

        CREATE INDEX IF NOT EXISTS idx_tasks_user_id ON tasks(user_id);
        """;

    private static string GetSeedSql() => """
        INSERT INTO users (id, name, email, password_hash, created_at, updated_at)
        VALUES
            ('11111111-1111-1111-1111-111111111111',
             'Demo User',
             'demo@taskmanager.com',
             '$2a$11$zGgokLRGTBcl2NsFTMZIIubwH2IYVHr7Ckj6ze2I417O87uNfqt42',
             NOW(), NOW())
        ON CONFLICT (email) DO NOTHING;

        INSERT INTO tasks (id, title, description, status, priority, due_date, user_id, created_at, updated_at)
        VALUES
            ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
             'Set up project repository',
             'Initialize git repo and configure CI/CD pipeline',
             2, 2, NULL,
             '11111111-1111-1111-1111-111111111111',
             NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days'),
            ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
             'Implement authentication',
             'JWT-based auth with refresh tokens',
             1, 3, NOW() + INTERVAL '2 days',
             '11111111-1111-1111-1111-111111111111',
             NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 day'),
            ('cccccccc-cccc-cccc-cccc-cccccccccccc',
             'Build task CRUD endpoints',
             'REST API with full CRUD operations',
             0, 2, NOW() + INTERVAL '5 days',
             '11111111-1111-1111-1111-111111111111',
             NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),
            ('dddddddd-dddd-dddd-dddd-dddddddddddd',
             'Write unit tests',
             'xUnit tests for all layers with Moq and FluentAssertions',
             0, 1, NOW() + INTERVAL '7 days',
             '11111111-1111-1111-1111-111111111111',
             NOW(), NOW())
        ON CONFLICT (id) DO NOTHING;
        """;
}
