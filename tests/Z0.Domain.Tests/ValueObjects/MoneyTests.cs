using FluentAssertions;
using Z0.Domain.ValueObjects;
using Xunit;

namespace Z0.Domain.Tests.ValueObjects;

[Trait("Category", "Unit")]
public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateMoney()
    {
        // Act
        var money = new Money(100m, "USD");

        // Assert
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithDefaultCurrency_ShouldUseUSD()
    {
        // Act
        var money = new Money(100m);

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Money(-100m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyCurrency_ShouldThrowArgumentException(string? currency)
    {
        // Act
        var act = () => new Money(100m, currency!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSummedMoney()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnSubtractedMoney()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Multiply_ShouldReturnMultipliedMoney()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money.Multiply(2.5m);

        // Assert
        result.Amount.Should().Be(250m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(1234.56m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("USD");
        result.Should().Contain("1,234.56");
    }

    [Fact]
    public void TwoMoneyWithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Assert
        money1.Should().Be(money2);
    }
}
