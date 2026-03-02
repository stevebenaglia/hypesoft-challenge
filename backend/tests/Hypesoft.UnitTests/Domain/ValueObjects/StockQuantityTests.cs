using FluentAssertions;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.ValueObjects;

namespace Hypesoft.UnitTests.Domain.ValueObjects;

public sealed class StockQuantityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(1_000_000)]
    public void Create_WithNonNegativeValue_ShouldSucceed(int value)
    {
        var stock = StockQuantity.Create(value);

        stock.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNegativeValue_ShouldThrowDomainException(int value)
    {
        var act = () => StockQuantity.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage("Stock quantity cannot be negative.");
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(9, true)]
    [InlineData(10, false)]
    [InlineData(100, false)]
    public void IsLow_ShouldReturnCorrectValue(int value, bool expectedIsLow)
    {
        var stock = StockQuantity.Create(value);

        stock.IsLow.Should().Be(expectedIsLow);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        var stock = StockQuantity.Create(42);

        int result = stock;

        result.Should().Be(42);
    }
}
