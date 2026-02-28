using Hypesoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Hypesoft.Infrastructure.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToCollection("products");
        // Index on CategoryId: speeds up filtering products by category
        modelBuilder.Entity<Product>().HasIndex(p => p.CategoryId);
        // Index on StockQuantity: speeds up low-stock queries and range filters
        modelBuilder.Entity<Product>().HasIndex(p => p.StockQuantity);
        // Compound index: covers the common case of filtering by category + sorting by name
        modelBuilder.Entity<Product>().HasIndex(p => new { p.CategoryId, p.Name });

        modelBuilder.Entity<Category>().ToCollection("categories");
        // Unique index on Name: enforces uniqueness at DB level and speeds up ExistsByNameAsync
        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
    }
}
