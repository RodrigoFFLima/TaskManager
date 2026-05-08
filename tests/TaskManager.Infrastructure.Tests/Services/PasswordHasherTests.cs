using FluentAssertions;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ReturnsNonEmptyHash()
    {
        var hash = _sut.Hash("Password1");

        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_DifferentCallsProduceDifferentHashes()
    {
        var hash1 = _sut.Hash("Password1");
        var hash2 = _sut.Hash("Password1");

        // BCrypt uses a random salt — same input yields different hashes
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("Password1");

        _sut.Verify("Password1", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("Password1");

        _sut.Verify("WrongPass1", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("Password1");

        _sut.Verify("", hash).Should().BeFalse();
    }
}
