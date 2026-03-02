using FluentValidation.TestHelper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Validators.Products;
using Hypesoft.Domain.Repositories;
using Moq;

namespace Hypesoft.UnitTests.Validators;

public sealed class CreateProductCommandValidatorTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _repoMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _validator = new CreateProductCommandValidator(_repoMock.Object);
    }

    private static CreateProductCommand ValidCommand() =>
        new("Produto Teste", null, 99.90m, 10, "cat-1");

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyName_ShouldFail(string? name)
    {
        var command = ValidCommand() with { Name = name! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Validate_WithNameExceeding200Chars_ShouldFail()
    {
        var command = ValidCommand() with { Name = new string('A', 201) };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Theory]
    [InlineData("<b>bold</b>")]
    [InlineData("Name <script>alert(1)</script>")]
    public async Task Validate_WithHtmlInName_ShouldFail(string name)
    {
        var command = ValidCommand() with { Name = name };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not contain HTML tags.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public async Task Validate_WithZeroOrNegativePrice_ShouldFail(decimal price)
    {
        var command = ValidCommand() with { Price = price };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than zero.");
    }

    [Fact]
    public async Task Validate_WithNegativeStockQuantity_ShouldFail()
    {
        var command = ValidCommand() with { StockQuantity = -1 };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
            .WithErrorMessage("Stock quantity must be zero or greater.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyCategoryId_ShouldFail(string? categoryId)
    {
        var command = ValidCommand() with { CategoryId = categoryId! };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("CategoryId is required.");
    }

    [Fact]
    public async Task Validate_WithNonExistentCategoryId_ShouldFail()
    {
        _repoMock
            .Setup(r => r.ExistsAsync("cat-invalid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = ValidCommand() with { CategoryId = "cat-invalid" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category not found.");
    }
}
