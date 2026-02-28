using FluentValidation;
using Hypesoft.Application.Queries.Products;

namespace Hypesoft.Application.Validators.Products;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("SearchTerm must not exceed 200 characters.")
            .When(x => x.SearchTerm is not null);
    }
}
