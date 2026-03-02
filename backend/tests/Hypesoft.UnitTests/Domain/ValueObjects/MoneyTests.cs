using FluentAssertions;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.ValueObjects;

namespace Hypesoft.UnitTests.Domain.ValueObjects;

public sealed class MoneyTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(999.99)]
    [InlineData(1_000_000)]
    public void Create_WithPositiveAmount_ShouldSucceed(decimal amount)
    {
        var money = Money.Create(amount);

        money.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegative_ShouldThrowDomainException(decimal amount)
    {
        var act = () => Money.Create(amount);

        act.Should().Throw<DomainException>()
            .WithMessage("Price must be greater than zero.");
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnAmount()
    {
        var money = Money.Create(42.50m);

        decimal result = money;

        result.Should().Be(42.50m);
    }
}
