using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoryByIdHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}
