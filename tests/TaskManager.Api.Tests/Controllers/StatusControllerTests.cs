using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Api.Tests.Helpers;

namespace TaskManager.Api.Tests.Controllers;

public class StatusControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public StatusControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Public_WithoutToken_Returns200()
    {
        var response = await _factory.CreateClient().GetAsync("/api/status/public");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"access\":\"public\"");
    }

    [Fact]
    public async Task Private_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/status/private");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Private_WithValidToken_Returns200WithUserClaims()
    {
        var userId = Guid.NewGuid();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(userId));

        var response = await client.GetAsync("/api/status/private");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"access\":\"private\"").And.Contain("\"user\"");
    }
}
