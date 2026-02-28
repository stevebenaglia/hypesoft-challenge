using Hypesoft.Domain.Exceptions;

namespace Hypesoft.Domain.ValueObjects;

/// <summary>Value object representing a monetary amount. Guarantees amount > 0.</summary>
public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Create(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Price must be greater than zero.");

        return new Money(amount);
    }

    public static implicit operator decimal(Money money) => money.Amount;

    public override string ToString() => Amount.ToString("C");
}
