using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepoMock.Object, _jwtMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        _userRepoMock.Setup(r => r.ExistsByEmailAsync("new@example.com", default))
            .ReturnsAsync(false);

        _hasherMock.Setup(h => h.Hash("Password1"))
            .Returns("hashed");

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .ReturnsAsync(Guid.NewGuid());

        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var request = new RegisterRequest("Test User", "new@example.com", "Password1");
        var result = await _sut.RegisterAsync(request);

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsDomainException()
    {
        _userRepoMock.Setup(r => r.ExistsByEmailAsync("taken@example.com", default))
            .ReturnsAsync(true);

        var request = new RegisterRequest("User", "taken@example.com", "Password1");
        var act = async () => await _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already registered*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var user = User.Create("Demo", "demo@example.com", "hashed");

        _userRepoMock.Setup(r => r.GetByEmailAsync("demo@example.com", default))
            .ReturnsAsync(user);

        _hasherMock.Setup(h => h.Verify("Password1", "hashed"))
            .Returns(true);

        _jwtMock.Setup(j => j.GenerateToken(user))
            .Returns("jwt-token");

        var result = await _sut.LoginAsync(new LoginRequest("demo@example.com", "Password1"));

        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = User.Create("Demo", "demo@example.com", "hashed");

        _userRepoMock.Setup(r => r.GetByEmailAsync("demo@example.com", default))
            .ReturnsAsync(user);

        _hasherMock.Setup(h => h.Verify("WrongPassword1", "hashed"))
            .Returns(false);

        var act = async () => await _sut.LoginAsync(new LoginRequest("demo@example.com", "WrongPassword1"));

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*Invalid*");
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ThrowsUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.LoginAsync(new LoginRequest("ghost@example.com", "Password1"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
