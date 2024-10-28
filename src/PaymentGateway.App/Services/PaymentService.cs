using PaymentGateway.App.Infra;
using PaymentGateway.App.Models.Domain;
using PaymentGateway.App.Models.Requests;
using PaymentGateway.App.Models.ValueTypes;
using PaymentGateway.Lib.Functional;

namespace PaymentGateway.App.Services;

public class PaymentService(PaymentsRepository paymentsRepository, IBankClient bankClient)
{
    public Opt<ExistingPayment> GetPaymentByRequest(PaymentRequestId id) =>
        paymentsRepository.GetPaymentFromRequest(id);

    public Opt<Payment> GetPayment(PaymentId id) => paymentsRepository.Get(id);

    public async Task<PaymentCreationResult> MakePayment(
        PaymentRequest paymentRequest,
        CancellationToken cancellationToken)
        =>
            (await bankClient.SendPayment(paymentRequest, cancellationToken))
            .Match<PaymentCreationResult>(
                bankResponse => paymentsRepository.AddPayment(new Payment(paymentRequest, bankResponse)),
                _ => new BadRequest("Invalid card details."),
                _ => new InternalError("An error occurred processing your payment."));
}
