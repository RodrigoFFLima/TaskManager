namespace TaskManager.Infrastructure.Data;

public interface IDatabaseMigrator
{
    Task MigrateAsync();
    Task SeedAsync();
}
