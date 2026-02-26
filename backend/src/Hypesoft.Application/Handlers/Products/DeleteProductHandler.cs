using Hypesoft.Application.Commands.Products;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        await _productRepository.DeleteAsync(product, cancellationToken);

        return Unit.Value;
    }
}
