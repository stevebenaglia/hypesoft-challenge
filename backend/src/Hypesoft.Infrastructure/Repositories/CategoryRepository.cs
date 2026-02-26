using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        => await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
}
