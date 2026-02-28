using FluentValidation;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Validators.Products;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .Matches(@"^[^<>]*$").WithMessage("Name must not contain HTML tags.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .Matches(@"^[^<>]*$").WithMessage("Description must not contain HTML tags.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be zero or greater.");

        // CategoryId is optional on update; if provided, it must reference an existing category.
        When(x => !string.IsNullOrEmpty(x.CategoryId), () =>
        {
            RuleFor(x => x.CategoryId)
                .MustAsync(async (id, ct) => await categoryRepository.ExistsAsync(id!, ct))
                .WithMessage("Category not found.");
        });
    }
}
