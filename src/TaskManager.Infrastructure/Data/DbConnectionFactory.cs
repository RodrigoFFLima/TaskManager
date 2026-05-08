using Npgsql;

namespace TaskManager.Infrastructure.Data;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}
