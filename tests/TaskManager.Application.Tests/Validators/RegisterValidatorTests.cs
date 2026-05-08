using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace TaskManager.Application.Tests.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _sut = new();

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var result = await _sut.ValidateAsync(new RegisterRequest("Alice", "alice@example.com", "Password1"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyName_Fails(string? name)
    {
        var result = await _sut.ValidateAsync(new RegisterRequest(name!, "a@b.com", "Password1"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("")]
    public async Task Validate_InvalidEmail_Fails(string email)
    {
        var result = await _sut.ValidateAsync(new RegisterRequest("Alice", email, "Password1"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("short1A")]          // 7 chars
    [InlineData("alllowercase1")]    // no uppercase
    [InlineData("ALLUPPERCASE1")]    // no lowercase
    [InlineData("NoNumbers!!!")]     // no digit
    public async Task Validate_WeakPassword_Fails(string password)
    {
        var result = await _sut.ValidateAsync(new RegisterRequest("Alice", "alice@example.com", password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("Abcdefg1")]
    [InlineData("ComplexP4ss")]
    public async Task Validate_StrongPassword_Passes(string password)
    {
        var result = await _sut.ValidateAsync(new RegisterRequest("Alice", "alice@example.com", password));

        result.IsValid.Should().BeTrue();
    }
}
