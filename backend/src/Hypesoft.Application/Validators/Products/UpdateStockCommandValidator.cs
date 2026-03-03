using FluentValidation;
using Hypesoft.Application.Commands.Products;

namespace Hypesoft.Application.Validators.Products;

public sealed class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade em estoque deve ser zero ou maior.")
            .LessThanOrEqualTo(1_000_000).WithMessage("A quantidade em estoque não pode ultrapassar 1.000.000.");
    }
}
