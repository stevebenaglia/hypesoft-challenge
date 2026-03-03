using FluentValidation;
using Hypesoft.Application.Queries.Products;

namespace Hypesoft.Application.Validators.Products;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("O número da página deve ser pelo menos 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("O tamanho da página deve estar entre 1 e 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("O termo de busca não pode ultrapassar 200 caracteres.")
            .When(x => x.SearchTerm is not null);
    }
}
