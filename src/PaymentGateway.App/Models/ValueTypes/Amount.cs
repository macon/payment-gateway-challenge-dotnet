using PaymentGateway.Lib.Functional;

namespace PaymentGateway.App.Models.ValueTypes;

public record Amount
{
    public Currency Currency { get; }
    public int AmountInMinor { get; }

    private Amount(int amount, Currency currency) => (Currency, AmountInMinor) = (currency, amount);

    public static Val<Amount> New(int amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) { return "Missing field: Currency"; }
        
        if (amount <= 0) { return $"Invalid amount: {amount}. Must be greater than 0."; }

        return Currency
            .From(currencyCode)
            .Map(currency => new Amount(amount, currency));
    }
}