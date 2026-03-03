using FluentValidation;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Validators.Products;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome não pode ultrapassar 200 caracteres.")
            .Matches(@"^[^<>]*$").WithMessage("O nome não pode conter tags HTML.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("A descrição não pode ultrapassar 500 caracteres.")
            .Matches(@"^[^<>]*$").WithMessage("A descrição não pode conter tags HTML.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade em estoque deve ser zero ou maior.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("A categoria é obrigatória.")
            .MustAsync(async (id, ct) => await categoryRepository.ExistsAsync(id, ct))
            .WithMessage("Categoria não encontrada.");
    }
}
