using PaymentGateway.App.Models.Bank.Responses;
using PaymentGateway.App.Models.Domain;
using PaymentGateway.App.Models.Requests;
using PaymentGateway.App.Models.ValueTypes;

using Xunit.Sdk;

namespace PaymentGateway.App.Tests.Shared;

public class PaymentBuilder
{
    public const string AuthorizationCode = "0bb07405-6d44-4b50-a14f-7ae0beff13ad";

    public static Payment GetPayment()
    {
        return new Payment(GetPaymentRequest(), GetBankResponse());
    }

    private static BankPaymentResponse GetBankResponse()
    {
        return new BankPaymentResponse(true, AuthorizationCode);
    }

    public static PaymentRequest GetPaymentRequest()
    {
        var creditCard = CreditCard.New(
            cardNumber: "2222405343248877",
            expiryMonth: 12,
            expiryYear: DateTime.UtcNow.Year + 1,
            cvv: "123");

        var paymentAmount = Amount.New(amount: 100, currencyCode: "GBP");

        var paymentRequest = PaymentRequest.New(
            creditCard,
            paymentAmount,
            Guid.NewGuid().ToString(),
            []);

        return paymentRequest.Match(
            err => throw new TestClassException($"Payment test data setup failed: {err}"),
            pr => pr);
    }
}