using System.Runtime.CompilerServices;

using PaymentGateway.Lib.Functional;

// ReSharper disable InconsistentNaming

// TODO: disclaimer: This is a bit naughty. It's only here to allow tests to create expired cards to populate the repository.
[assembly: InternalsVisibleTo("PaymentGateway.Api.Tests")]

namespace PaymentGateway.App.Models.ValueTypes;

public record CreditCard
{
    public string CardNumber { get; }
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public string CVV { get; }
    public string LastFourDigits => CardNumber[^4..];
    public string ExpiryDate => $"{ExpiryMonth:D2}/{ExpiryYear}";

    private CreditCard(string cardNumber, int expiryMonth, int expiryYear, string cvv)
    {
        CardNumber = cardNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CVV = cvv;
    }
    
    private static readonly Func<int, int, bool> IsCardExpired = 
        (m, y) => DateTime.UtcNow > new DateTime(new DateTime(y, m, 1).AddMonths(1).Ticks-1);

    public static Val<CreditCard> New(string cardNumber, int expiryMonth, int expiryYear, string cvv)
    {
        return NewCard(cardNumber, expiryMonth, expiryYear, cvv, IsCardExpired);
    }
    
    internal static Val<CreditCard> New(string cardNumber, int expiryMonth, int expiryYear, string cvv, Func<int, int, bool> isCardExpired)
    {
        return NewCard(cardNumber, expiryMonth, expiryYear, cvv, isCardExpired);
    }
    
    // TODO: Could break this up into more modular rules and then combine.
    private static Val<CreditCard> NewCard(string cardNumber, int expiryMonth, int expiryYear, string cvv, Func<int, int, bool> isCardExpired)
    {
        if (cardNumber is null) { return "Missing field: CardNumber"; }
        if (cvv is null) { return "Missing field: CVV"; }
        
        if (cardNumber.Length is < 14 or > 19)
        {
            return $"Invalid card number: {cardNumber}. Must be between 14 and 19 digits.";
        }
        
        if (!cardNumber.All(char.IsDigit))
        {
            return $"Invalid card number: {cardNumber}. Can only contain digits.";
        }

        if (expiryMonth is < 1 or > 12)
        {
            return $"Invalid expiry month: {expiryMonth}. Must be between 1 and 12.";
        }
        
        if (expiryYear < DateTime.MinValue.Year || expiryYear > DateTime.MaxValue.Year)
        {
            return $"Invalid expiry year: {expiryYear}. Must be between 1 and 9999.";
        }

        if (isCardExpired(expiryMonth, expiryYear))
        {
            return $"Card has expired: {expiryMonth}/{expiryYear}.";
        }

        if (cvv.Length is < 3 or > 4)
        {
            return $"Invalid CVV: {cvv}. Must be between 3 and 4 digits.";
        }
        
        if (!cvv.All(char.IsDigit))
        {
            return $"Invalid CVV: {cvv}. Can only contain digits.";
        }
        
        return new CreditCard(cardNumber, expiryMonth, expiryYear, cvv);
    }
}
