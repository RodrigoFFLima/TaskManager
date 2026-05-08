using FluentAssertions;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("  user@example.com  ")]
    public void Create_ValidEmail_ReturnsNormalizedEmail(string input)
    {
        var email = Email.Create(input);
        email.Value.Should().Be("user@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyEmail_ThrowsDomainException(string? input)
    {
        var act = () => Email.Create(input!);
        act.Should().Throw<DomainException>().WithMessage("*empty*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("nodomain@")]
    public void Create_InvalidFormat_ThrowsDomainException(string input)
    {
        var act = () => Email.Create(input);
        act.Should().Throw<DomainException>().WithMessage("*invalid*");
    }

    [Fact]
    public void Equals_SameEmail_ReturnsTrue()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeFalse(); // reference equality, not overloaded ==
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var email = Email.Create("user@example.com");
        string str = email;
        str.Should().Be("user@example.com");
    }
}
