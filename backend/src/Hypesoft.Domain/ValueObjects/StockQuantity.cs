using Hypesoft.Domain.Exceptions;

namespace Hypesoft.Domain.ValueObjects;

/// <summary>Value object representing a non-negative stock quantity.</summary>
public sealed record StockQuantity
{
    private const int LowStockThreshold = 10;

    public int Value { get; }

    private StockQuantity(int value) => Value = value;

    public static StockQuantity Create(int value)
    {
        if (value < 0)
            throw new DomainException("Stock quantity cannot be negative.");

        return new StockQuantity(value);
    }

    public bool IsLow => Value < LowStockThreshold;

    public static implicit operator int(StockQuantity stock) => stock.Value;

    public override string ToString() => Value.ToString();
}
