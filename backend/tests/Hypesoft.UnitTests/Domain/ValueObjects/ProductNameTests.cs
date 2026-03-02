using FluentAssertions;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.ValueObjects;

namespace Hypesoft.UnitTests.Domain.ValueObjects;

public sealed class ProductNameTests
{
    [Theory]
    [InlineData("Produto A")]
    [InlineData("  Produto com espaços  ")]
    [InlineData("X")]
    public void Create_WithValidName_ShouldSucceed(string name)
    {
        var productName = ProductName.Create(name);

        productName.Value.Should().Be(name.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespace_ShouldThrowDomainException(string? name)
    {
        var act = () => ProductName.Create(name!);

        act.Should().Throw<DomainException>()
            .WithMessage("Product name cannot be empty.");
    }

    [Fact]
    public void Create_WithNameExceeding200Chars_ShouldThrowDomainException()
    {
        var longName = new string('A', 201);

        var act = () => ProductName.Create(longName);

        act.Should().Throw<DomainException>()
            .WithMessage($"Product name cannot exceed {ProductName.MaxLength} characters.");
    }

    [Fact]
    public void Create_WithExactly200Chars_ShouldSucceed()
    {
        var name = new string('A', 200);

        var productName = ProductName.Create(name);

        productName.Value.Should().HaveLength(200);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnValue()
    {
        var productName = ProductName.Create("Test");

        string result = productName;

        result.Should().Be("Test");
    }
}
