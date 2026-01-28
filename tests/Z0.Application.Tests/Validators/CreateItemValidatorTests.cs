using FluentAssertions;
using Z0.Application.DTOs.Items;
using Z0.Application.Validators;
using Xunit;

namespace Z0.Application.Tests.Validators;

[Trait("Category", "Unit")]
public class CreateItemValidatorTests
{
    private readonly CreateItemValidator _validator;

    public CreateItemValidatorTests()
    {
        _validator = new CreateItemValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateItemRequest("Test Item", "Test Description");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var request = new CreateItemRequest("", "Test Description");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithNameExceeding200Characters_ShouldFail()
    {
        // Arrange
        var request = new CreateItemRequest(new string('A', 201), "Test Description");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("200 characters"));
    }

    [Fact]
    public async Task Validate_WithNullDescription_ShouldPass()
    {
        // Arrange
        var request = new CreateItemRequest("Test Item", null);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithDescriptionExceeding1000Characters_ShouldFail()
    {
        // Arrange
        var request = new CreateItemRequest("Test Item", new string('A', 1001));

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
