using FluentAssertions;
using Hypesoft.Domain.Entities;

namespace Hypesoft.UnitTests.Domain.Entities;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetAllProperties()
    {
        var category = Category.Create("id-1", "Eletrônicos", "Produtos eletrônicos");

        category.Id.Should().Be("id-1");
        category.Name.Should().Be("Eletrônicos");
        category.Description.Should().Be("Produtos eletrônicos");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceDescription_ShouldNormalizeToNull(string? description)
    {
        var category = Category.Create("id-1", "Eletrônicos", description);

        category.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutDescription_ShouldHaveNullDescription()
    {
        var category = Category.Create("id-1", "Eletrônicos");

        category.Description.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldChangeName()
    {
        var category = Category.Create("id-1", "Original");

        category.Update("Atualizado", null);

        category.Name.Should().Be("Atualizado");
    }

    [Fact]
    public void Update_WithValidDescription_ShouldUpdateDescription()
    {
        var category = Category.Create("id-1", "Nome", "descrição antiga");

        category.Update("Nome", "descrição nova");

        category.Description.Should().Be("descrição nova");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyDescription_ShouldNormalizeToNull(string? description)
    {
        var category = Category.Create("id-1", "Nome", "descrição antiga");

        category.Update("Nome", description);

        category.Description.Should().BeNull();
    }
}
