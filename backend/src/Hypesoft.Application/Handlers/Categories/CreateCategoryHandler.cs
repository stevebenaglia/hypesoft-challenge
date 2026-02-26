using AutoMapper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly IMapper _mapper;

    public CreateCategoryHandler(
        ICategoryRepository categoryRepository,
        IIdGenerator idGenerator,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _idGenerator = idGenerator;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(_idGenerator.NewId(), request.Name, request.Description);

        await _categoryRepository.AddAsync(category, cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}
