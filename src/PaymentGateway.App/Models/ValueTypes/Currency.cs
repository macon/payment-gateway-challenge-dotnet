using PaymentGateway.Lib.Functional;
// ReSharper disable InconsistentNaming

namespace PaymentGateway.App.Models.ValueTypes;

public record Currency
{
    public const string GbpValue = "GBP";
    public const string EurValue = "EUR";
    public const string UsdValue = "USD";
    
    public string Name { get; }
    private Currency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency is not (GbpValue or EurValue or UsdValue))
        {
            throw new ArgumentOutOfRangeException(nameof(currency), currency, "Invalid currency");
        }

        Name = currency;
    }

    private static Currency GBP { get; } = new(GbpValue);
    private static Currency EUR { get; } = new(EurValue);
    private static Currency USD { get; } = new(UsdValue);

    private static IEnumerable<string> ValidCodes => [GbpValue, EurValue, UsdValue];

    public static Val<Currency> From(string currencyCode) =>
        currencyCode.ToUpper() switch
        {
            GbpValue => Valid(GBP),
            EurValue => Valid(EUR),
            UsdValue => Valid(USD),
            _ => $"Invalid currency code: {currencyCode}. Valid currencies: {string.Join(',', ValidCodes)}"
        };

    public override string ToString() => Name;
    public static implicit operator string(Currency value) => value.Name;
}