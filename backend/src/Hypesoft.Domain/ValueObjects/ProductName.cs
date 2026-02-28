using Hypesoft.Domain.Exceptions;

namespace Hypesoft.Domain.ValueObjects;

/// <summary>Value object representing a product name. Must be non-empty and at most 200 characters.</summary>
public sealed record ProductName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ProductName(string value) => Value = value;

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product name cannot be empty.");

        if (value.Length > MaxLength)
            throw new DomainException($"Product name cannot exceed {MaxLength} characters.");

        return new ProductName(value.Trim());
    }

    public static implicit operator string(ProductName name) => name.Value;

    public override string ToString() => Value;
}
