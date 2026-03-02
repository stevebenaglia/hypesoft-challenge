using FluentAssertions;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.ValueObjects;

namespace Hypesoft.UnitTests.Domain.Entities;

public sealed class ProductTests
{
    private static Product BuildProduct(
        string id = "prod-1",
        string name = "Produto Teste",
        string? description = null,
        decimal price = 99.90m,
        int stock = 15,
        string categoryId = "cat-1")
    {
        return Product.Create(
            id,
            ProductName.Create(name),
            description,
            Money.Create(price),
            StockQuantity.Create(stock),
            categoryId);
    }

    [Fact]
    public void Create_WithValidData_ShouldSetAllProperties()
    {
        var product = BuildProduct(description: "Uma descrição");

        product.Id.Should().Be("prod-1");
        product.Name.Should().Be("Produto Teste");
        product.Description.Should().Be("Uma descrição");
        product.Price.Should().Be(99.90m);
        product.StockQuantity.Should().Be(15);
        product.CategoryId.Should().Be("cat-1");
    }

    [Fact]
    public void Update_ShouldChangeAllFields()
    {
        var product = BuildProduct();

        product.Update(
            ProductName.Create("Novo Nome"),
            "Nova descrição",
            Money.Create(49.99m),
            StockQuantity.Create(5),
            "cat-2");

        product.Name.Should().Be("Novo Nome");
        product.Description.Should().Be("Nova descrição");
        product.Price.Should().Be(49.99m);
        product.StockQuantity.Should().Be(5);
        product.CategoryId.Should().Be("cat-2");
    }

    [Fact]
    public void UpdateStock_ShouldChangeStockQuantity()
    {
        var product = BuildProduct(stock: 10);

        product.UpdateStock(StockQuantity.Create(3));

        product.StockQuantity.Should().Be(3);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(9, true)]
    [InlineData(10, false)]
    [InlineData(50, false)]
    public void IsLowStock_ShouldReturnCorrectValue(int stockQty, bool expectedLow)
    {
        var product = BuildProduct(stock: stockQty);

        product.IsLowStock().Should().Be(expectedLow);
    }
}
