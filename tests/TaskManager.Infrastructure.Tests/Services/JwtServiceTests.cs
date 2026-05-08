using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Infrastructure.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _sut;
    private const string Secret = "TestSecret_AtLeast32Characters_XYZ!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    public JwtServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"]         = Secret,
                ["Jwt:Issuer"]         = Issuer,
                ["Jwt:Audience"]       = Audience,
                ["Jwt:ExpiryMinutes"]  = "60"
            })
            .Build();

        _sut = new JwtService(config);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");

        var token = _sut.GenerateToken(user);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_TokenIsValidJwt()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));

        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);

        principal.Should().NotBeNull();
    }

    [Fact]
    public void GenerateToken_ContainsSubClaim_WithUserId()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");

        var token = _sut.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        sub.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");

        var token = _sut.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        email.Should().Be("alice@example.com");
    }

    [Fact]
    public void GenerateToken_ExpiresInExpectedWindow()
    {
        var user = User.Create("Alice", "alice@example.com", "hash");

        var token = _sut.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Constructor_MissingSecret_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var act = () => new JwtService(config);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Jwt:Secret*");
    }
}
