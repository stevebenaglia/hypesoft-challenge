using FluentValidation;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Validators.Categories;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .MustAsync(async (cmd, name, ct) => !await categoryRepository.ExistsByNameAsync(name, cmd.Id, ct))
            .WithMessage("A category with this name already exists.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
