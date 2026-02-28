using FluentValidation;
using Hypesoft.Application.Commands.Products;

namespace Hypesoft.Application.Validators.Products;

public sealed class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be zero or greater.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Stock quantity must not exceed 1,000,000.");
    }
}
