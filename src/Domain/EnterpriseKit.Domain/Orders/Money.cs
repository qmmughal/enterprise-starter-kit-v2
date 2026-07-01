namespace EnterpriseKit.Domain.Orders;

using EnterpriseKit.Domain.Common;

/// <summary>
/// Money value object — immutable, currency-aware monetary amount.
/// Two Money instances are equal only if both Amount and Currency match.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }  // ISO 4217 (e.g., "USD", "EUR")

    // Private EF Core constructor
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Required by EF Core (owned entity hydration)
    private Money() { Amount = 0; Currency = string.Empty; }

    /// <summary>Creates a new Money value, validating amount and currency.</summary>
    public static Money Of(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code is required.", nameof(currency));
        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>Adds two Money values. Currencies must match.</summary>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}.");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>Multiplies by a positive integer quantity.</summary>
    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

        return new Money(Amount * quantity, Currency);
    }

    public static Money Zero(string currency) => new(0, currency.ToUpperInvariant());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
