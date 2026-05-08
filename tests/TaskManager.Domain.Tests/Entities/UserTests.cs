using FluentAssertions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ValidData_ReturnsUser()
    {
        var user = User.Create("Alice", "alice@example.com", "hashed-password");

        user.Name.Should().Be("Alice");
        user.Email.Value.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyName_ThrowsDomainException(string? name)
    {
        var act = () => User.Create(name!, "user@example.com", "hash");
        act.Should().Throw<DomainException>().WithMessage("*Name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_EmptyPasswordHash_ThrowsDomainException(string? hash)
    {
        var act = () => User.Create("Alice", "alice@example.com", hash!);
        act.Should().Throw<DomainException>().WithMessage("*Password hash*");
    }

    [Fact]
    public void Create_InvalidEmail_ThrowsDomainException()
    {
        var act = () => User.Create("Alice", "not-an-email", "hash");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TrimsName()
    {
        var user = User.Create("  Alice  ", "alice@example.com", "hash");
        user.Name.Should().Be("Alice");
    }

    [Fact]
    public void UpdateName_ValidName_ChangesName()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");
        var before = user.UpdatedAt;

        user.UpdateName("Bob");

        user.Name.Should().Be("Bob");
        user.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void UpdateName_EmptyName_ThrowsDomainException()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");

        var act = () => user.UpdateName("   ");

        act.Should().Throw<DomainException>().WithMessage("*Name*");
    }

    [Fact]
    public void Reconstitute_RebuildsUserFromPersistence()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var user = User.Reconstitute(id, "Alice", "alice@example.com", "hash", now, now);

        user.Id.Should().Be(id);
        user.Name.Should().Be("Alice");
        user.Email.Value.Should().Be("alice@example.com");
    }
}
