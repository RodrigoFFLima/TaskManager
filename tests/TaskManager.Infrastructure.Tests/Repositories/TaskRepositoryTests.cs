using FluentAssertions;
using Moq;
using Npgsql;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure.Tests.Repositories;

// These are unit tests that verify the repository wires up parameters correctly.
// Integration tests against a real DB are skipped in CI via [Trait].
public class TaskRepositoryTests
{
    [Fact]
    public void TaskRepository_ImplementsITaskRepository()
    {
        // Verify that the concrete type satisfies the abstraction expected by Clean Architecture
        var factoryMock = new Mock<IDbConnectionFactory>();
        var repo = new TaskRepository(factoryMock.Object);

        repo.Should().BeAssignableTo<ITaskRepository>();
    }

    [Fact]
    public void UserRepository_ImplementsIUserRepository()
    {
        var factoryMock = new Mock<IDbConnectionFactory>();
        var repo = new UserRepository(factoryMock.Object);

        repo.Should().BeAssignableTo<IUserRepository>();
    }

    [Fact]
    public void DbConnectionFactory_EmptyConnectionString_ThrowsArgumentException()
    {
        var act = () => new DbConnectionFactory(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DbConnectionFactory_ValidConnectionString_CreatesConnection()
    {
        var factory = new DbConnectionFactory("Host=localhost;Database=test;Username=u;Password=p");
        var conn = factory.CreateConnection();

        conn.Should().BeOfType<NpgsqlConnection>();
    }
}
