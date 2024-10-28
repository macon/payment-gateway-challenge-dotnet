using PaymentGateway.App.Models.Requests;

namespace PaymentGateway.App.Models.Bank.Requests;

public static class BankClientExt
{
    public static BankPaymentRequest ToBankPaymentRequest(this PaymentRequest paymentRequest) =>
        new()
        {
            CardNumber = paymentRequest.CreditCard.CardNumber,
            ExpiryDate = paymentRequest.CreditCard.ExpiryDate,
            Cvv = paymentRequest.CreditCard.CVV,
            Amount = paymentRequest.Amount.AmountInMinor,
            Currency = paymentRequest.Amount.Currency
        };
}