using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace TaskManager.Application.Tests.Validators;

public class CreateTaskValidatorTests
{
    private readonly CreateTaskValidator _sut = new();

    [Fact]
    public async Task Validate_ValidRequest_PassesWithNoErrors()
    {
        var request = new CreateTaskRequest("Valid Title", "Some description", "High", null);

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_EmptyTitle_FailsWithTitleError(string title)
    {
        var result = await _sut.ValidateAsync(new CreateTaskRequest(title, null, "Medium", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_TitleExceeds200Chars_FailsValidation()
    {
        var request = new CreateTaskRequest(new string('x', 201), null, "Medium", null);

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("urgent")]
    [InlineData("")]
    public async Task Validate_InvalidPriority_FailsValidation(string priority)
    {
        var result = await _sut.ValidateAsync(new CreateTaskRequest("Title", null, priority, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public async Task Validate_AllValidPriorities_Pass(string priority)
    {
        var result = await _sut.ValidateAsync(new CreateTaskRequest("Title", null, priority, null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_PastDueDate_FailsValidation()
    {
        var request = new CreateTaskRequest("Title", null, "Medium", DateTime.UtcNow.AddDays(-1));

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public async Task Validate_FutureDueDate_Passes()
    {
        var request = new CreateTaskRequest("Title", null, "Medium", DateTime.UtcNow.AddDays(7));

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_DescriptionExceeds2000Chars_FailsValidation()
    {
        var request = new CreateTaskRequest("Title", new string('x', 2001), "Low", null);

        var result = await _sut.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_NullDueDate_Passes()
    {
        var result = await _sut.ValidateAsync(new CreateTaskRequest("Title", null, "Low", null));

        result.IsValid.Should().BeTrue();
    }
}
