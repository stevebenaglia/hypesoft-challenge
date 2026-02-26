using Hypesoft.Application.Commands.Categories;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public DeleteCategoryHandler(ICategoryRepository categoryRepository, IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        var hasProducts = await _productRepository.HasProductsInCategoryAsync(request.Id, cancellationToken);
        if (hasProducts)
            throw new DomainException($"Category '{category.Name}' cannot be deleted because it has associated products.");

        await _categoryRepository.DeleteAsync(category, cancellationToken);

        return Unit.Value;
    }
}
