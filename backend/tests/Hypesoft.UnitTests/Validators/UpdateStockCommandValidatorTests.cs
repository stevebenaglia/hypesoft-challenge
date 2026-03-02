using FluentAssertions;
using FluentValidation.TestHelper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Validators.Products;

namespace Hypesoft.UnitTests.Validators;

public sealed class UpdateStockCommandValidatorTests
{
    private readonly UpdateStockCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(999_999)]
    [InlineData(1_000_000)]
    public async Task Validate_WithValidQuantity_ShouldPass(int quantity)
    {
        var command = new UpdateStockCommand("id", quantity);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_WithNegativeQuantity_ShouldFail(int quantity)
    {
        var command = new UpdateStockCommand("id", quantity);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Stock quantity must be zero or greater.");
    }

    [Fact]
    public async Task Validate_WithQuantityExceedingMax_ShouldFail()
    {
        var command = new UpdateStockCommand("id", 1_000_001);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Stock quantity must not exceed 1,000,000.");
    }
}
