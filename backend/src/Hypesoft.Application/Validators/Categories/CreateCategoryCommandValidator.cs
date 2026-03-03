using FluentValidation;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Validators.Categories;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome não pode ultrapassar 100 caracteres.")
            .Matches(@"^[^<>]*$").WithMessage("O nome não pode conter tags HTML.")
            .MustAsync(async (name, ct) => !await categoryRepository.ExistsByNameAsync(name, null, ct))
            .WithMessage("Já existe uma categoria com este nome.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("A descrição não pode ultrapassar 500 caracteres.")
            .Matches(@"^[^<>]*$").WithMessage("A descrição não pode conter tags HTML.")
            .When(x => x.Description is not null);
    }
}
