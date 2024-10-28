using PaymentGateway.App.Models.Domain;
using PaymentGateway.App.Models.ValueTypes;
using PaymentGateway.Lib.Functional;
namespace PaymentGateway.App.Infra;

public record ExistingPayment(Payment Payment);

public class PaymentsRepository
{
    private readonly Dictionary<PaymentRequestId, PaymentId> _requestToPayments = new();
    private readonly Dictionary<PaymentId, Payment> _payments = new();

    public Payment AddPayment(Payment payment)
    {
        _requestToPayments[payment.PaymentRequest.Id] = payment.Id;
        _payments.Add(payment.Id, payment);
        return payment;
    }

    public Opt<ExistingPayment> GetPaymentFromRequest(PaymentRequestId paymentRequestId) =>
        _requestToPayments.TryGetValue(paymentRequestId, out var paymentId)
            ? Some(new ExistingPayment(_payments[paymentId]))
            : None;

    public Opt<Payment> Get(PaymentId id) => _payments.GetValueOrDefault(id);
}