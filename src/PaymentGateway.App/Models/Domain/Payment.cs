using PaymentGateway.App.Models.Bank.Responses;
using PaymentGateway.App.Models.Requests;
using PaymentGateway.App.Models.ValueTypes;

namespace PaymentGateway.App.Models.Domain;

public record Payment
{
    public PaymentId Id { get; }
    public PaymentRequest PaymentRequest { get; }
    public PaymentStatus Status { get; }

    public Payment(PaymentRequest paymentRequest, BankPaymentResponse bankResponse)
    {
        Id = PaymentId.New();
        PaymentRequest = paymentRequest;
        Status = bankResponse.BankAuthorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
    }

    public static Payment New(PaymentRequest paymentRequest, BankPaymentResponse bankResponse) =>
        new(paymentRequest, bankResponse);
}
