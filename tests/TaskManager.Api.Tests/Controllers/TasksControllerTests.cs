using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Api.Tests.Helpers;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Tests.Controllers;

public class TasksControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly HttpClient _authenticatedClient;
    private readonly Guid _userId = Guid.NewGuid();

    public TasksControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _taskServiceMock = factory.TaskServiceMock;

        _authenticatedClient = factory.CreateClient();
        _authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken(_userId));
    }

    // ── GET /api/tasks ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithExpiredToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateExpiredToken());

        var response = await client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithValidToken_Returns200WithTasks()
    {
        var tasks = new List<TaskDto>
        {
            new(Guid.NewGuid(), "Task A", null, "Pending", "High", null, _userId, DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), "Task B", "Desc", "InProgress", "Medium", null, _userId, DateTime.UtcNow, DateTime.UtcNow)
        };

        _taskServiceMock.Setup(s => s.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var response = await _authenticatedClient.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_EmptyList_Returns200WithEmptyArray()
    {
        _taskServiceMock.Setup(s => s.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskDto>());

        var response = await _authenticatedClient.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        body.Should().BeEmpty();
    }

    // ── GET /api/tasks/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync($"/api/tasks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_TaskExists_Returns200()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskDto(taskId, "My Task", null, "Pending", "Low", null, _userId, DateTime.UtcNow, DateTime.UtcNow);

        _taskServiceMock.Setup(s => s.GetByIdAsync(taskId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var response = await _authenticatedClient.GetAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TaskDto>();
        body!.Id.Should().Be(taskId);
    }

    [Fact]
    public async Task GetById_TaskNotFound_Returns404()
    {
        _taskServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Task not found."));

        var response = await _authenticatedClient.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/tasks ───────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Title", null, "Medium", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocation()
    {
        var newId = Guid.NewGuid();
        var created = new TaskDto(newId, "New Task", "Desc", "Pending", "High",
            null, _userId, DateTime.UtcNow, DateTime.UtcNow);

        _taskServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateTaskRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _authenticatedClient.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("New Task", "Desc", "High", null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<TaskDto>();
        body!.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task Create_ValidationFails_Returns400WithErrors()
    {
        _taskServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateTaskRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Title is required."));

        var response = await _authenticatedClient.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("", null, "Medium", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_LocationHeaderPointsToNewTask()
    {
        var newId = Guid.NewGuid();
        var created = new TaskDto(newId, "Task", null, "Pending", "Medium",
            null, _userId, DateTime.UtcNow, DateTime.UtcNow);

        _taskServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateTaskRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _authenticatedClient.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Task", null, "Medium", null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().EndWith($"/api/tasks/{newId}");
    }

    [Fact]
    public async Task GetById_InvalidGuidFormat_Returns404()
    {
        // The {id:guid} route constraint means non-GUID values don't match any route at all —
        // ASP.NET Core returns 404 (no route matched) rather than 400 (model binding failure).
        var response = await _authenticatedClient.GetAsync("/api/tasks/not-a-valid-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/tasks/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}",
            new UpdateTaskRequest("T", null, "Low", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_TaskExists_Returns200()
    {
        var taskId = Guid.NewGuid();
        var updated = new TaskDto(taskId, "Updated", null, "Pending", "High", null, _userId, DateTime.UtcNow, DateTime.UtcNow);

        _taskServiceMock.Setup(s => s.UpdateAsync(taskId, It.IsAny<UpdateTaskRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var response = await _authenticatedClient.PutAsJsonAsync($"/api/tasks/{taskId}",
            new UpdateTaskRequest("Updated", null, "High", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_TaskNotFound_Returns404()
    {
        _taskServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateTaskRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Task not found."));

        var response = await _authenticatedClient.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}",
            new UpdateTaskRequest("T", null, "Low", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ValidationFails_Returns400()
    {
        _taskServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateTaskRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Title is required."));

        var response = await _authenticatedClient.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}",
            new UpdateTaskRequest("", null, "Low", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PATCH /api/tasks/{id}/status ─────────────────────────────────────────

    [Fact]
    public async Task ChangeStatus_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/status", new ChangeStatusRequest("InProgress"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeStatus_ValidTransition_Returns200()
    {
        var taskId = Guid.NewGuid();
        var updated = new TaskDto(taskId, "T", null, "InProgress", "Medium", null, _userId, DateTime.UtcNow, DateTime.UtcNow);

        _taskServiceMock.Setup(s => s.ChangeStatusAsync(taskId, It.IsAny<ChangeStatusRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var response = await _authenticatedClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status", new ChangeStatusRequest("InProgress"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TaskDto>();
        body!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task ChangeStatus_InvalidTransition_Returns422()
    {
        _taskServiceMock.Setup(s => s.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<ChangeStatusRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Cannot transition from 'Pending' to 'Completed'."));

        var response = await _authenticatedClient.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/status", new ChangeStatusRequest("Completed"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangeStatus_TaskNotFound_Returns404()
    {
        _taskServiceMock.Setup(s => s.ChangeStatusAsync(It.IsAny<Guid>(), It.IsAny<ChangeStatusRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Task not found."));

        var response = await _authenticatedClient.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/status", new ChangeStatusRequest("InProgress"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/tasks/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().DeleteAsync($"/api/tasks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_TaskExists_Returns204()
    {
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(s => s.DeleteAsync(taskId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _authenticatedClient.DeleteAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_TaskNotFound_Returns404()
    {
        _taskServiceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Task not found."));

        var response = await _authenticatedClient.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
