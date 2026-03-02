using FluentValidation.TestHelper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.Validators.Categories;
using Hypesoft.Domain.Repositories;
using Moq;

namespace Hypesoft.UnitTests.Validators;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly CreateCategoryCommandValidator _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _repoMock
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _validator = new CreateCategoryCommandValidator(_repoMock.Object);
    }

    [Fact]
    public async Task Validate_WithValidData_ShouldPass()
    {
        var command = new CreateCategoryCommand("Eletrônicos", null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithValidNameAndDescription_ShouldPass()
    {
        var command = new CreateCategoryCommand("Eletrônicos", "Produtos eletrônicos em geral");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyName_ShouldFail(string? name)
    {
        var command = new CreateCategoryCommand(name!, null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Validate_WithNameExceeding100Chars_ShouldFail()
    {
        var command = new CreateCategoryCommand(new string('A', 101), null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 100 characters.");
    }

    [Theory]
    [InlineData("<script>")]
    [InlineData("Name <b>bold</b>")]
    [InlineData(">Injection")]
    public async Task Validate_WithHtmlInName_ShouldFail(string name)
    {
        var command = new CreateCategoryCommand(name, null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not contain HTML tags.");
    }

    [Fact]
    public async Task Validate_WithDuplicateName_ShouldFail()
    {
        _repoMock
            .Setup(r => r.ExistsByNameAsync("Eletrônicos", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCategoryCommand("Eletrônicos", null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A category with this name already exists.");
    }

    [Fact]
    public async Task Validate_WithDescriptionExceeding500Chars_ShouldFail()
    {
        var command = new CreateCategoryCommand("Nome", new string('A', 501));

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters.");
    }

    [Theory]
    [InlineData("<p>html</p>")]
    [InlineData("desc <b>em negrito</b>")]
    public async Task Validate_WithHtmlInDescription_ShouldFail(string description)
    {
        var command = new CreateCategoryCommand("Nome", description);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not contain HTML tags.");
    }
}
