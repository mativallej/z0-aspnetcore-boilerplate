using FluentAssertions;
using Z0.Application.DTOs.Products;
using Z0.Application.Validators;
using Xunit;

namespace Z0.Application.Tests.Validators;

[Trait("Category", "Unit")]
public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator;

    public CreateProductValidatorTests()
    {
        _validator = new CreateProductValidator();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            Description = "Test Description"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptySku_ShouldFail()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "",
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public async Task Validate_WithSkuExceeding50Characters_ShouldFail()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = new string('A', 51),
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku" && e.ErrorMessage.Contains("50 characters"));
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithNegativePrice_ShouldFail()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Test Product",
            Price = -1m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_WithZeroPrice_ShouldPass()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Test Product",
            Price = 0m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldFail()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = Guid.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public async Task Validate_WithDescriptionExceeding2000Characters_ShouldFail()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Sku = "SKU-001",
            Name = "Test Product",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            Description = new string('A', 2001)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
