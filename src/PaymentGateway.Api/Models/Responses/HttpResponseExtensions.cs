using PaymentGateway.App.Models;
using PaymentGateway.App.Models.Domain;

namespace PaymentGateway.Api.Models.Responses;

public static class HttpResponseExtensions
{
    public static PaymentResponse ToPostPaymentResponse(this Payment payment) =>
        new()
        {
            Id = payment.Id,
            CardNumberLastFour = payment.PaymentRequest.CreditCard.LastFourDigits,
            ExpiryMonth = payment.PaymentRequest.CreditCard.ExpiryMonth,
            ExpiryYear = payment.PaymentRequest.CreditCard.ExpiryYear,
            Amount = payment.PaymentRequest.Amount.AmountInMinor,
            Currency = payment.PaymentRequest.Amount.Currency,
            Status = payment.Status
        };
}