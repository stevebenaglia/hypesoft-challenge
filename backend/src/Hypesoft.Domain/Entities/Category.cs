namespace Hypesoft.Domain.Entities;

public sealed class Category
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>Required by EF Core for materialization.</summary>
    private Category() { }

    public static Category Create(string id, string name, string? description = null)
    {
        return new Category
        {
            Id = id,
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
