namespace UserService.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public const decimal MaxAmount = 9_999_999_999_999_999.99m; // 18 digits total (16.2)

    public static Money Create(decimal amount, string currency = "VND")
    {
        if (amount < 0)
            throw new UserService.Domain.Exceptions.InvalidDomainObjectException("Số tiền không được âm.");

        if (amount > MaxAmount)
            throw new UserService.Domain.Exceptions.InvalidDomainObjectException($"Số tiền không được vượt quá {MaxAmount:N0}.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new UserService.Domain.Exceptions.InvalidDomainObjectException("Tiền tệ không được để trống.");

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    public static implicit operator decimal(Money money) => money.Amount;
}
