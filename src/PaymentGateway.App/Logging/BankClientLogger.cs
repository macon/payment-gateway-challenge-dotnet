using Microsoft.Extensions.Logging;

using PaymentGateway.App.Infra;
using PaymentGateway.App.Models.Requests;

namespace PaymentGateway.App.Logging;

public class BankClientLogger(IBankClient bankClient, ILogger<BankClientLogger> logger) : IBankClient
{
    public async Task<BankCallResult> SendPayment(PaymentRequest paymentRequest, CancellationToken cancellationToken)
    {
        logger.LogInformation("[Payment:{PaymentId}] Sending payment to bank", paymentRequest.Id);

        return (await bankClient.SendPayment(paymentRequest, cancellationToken))
            .Match<BankCallResult>(
                bankPaymentResponse =>
                {
                    logger.LogInformation("[Payment:{PaymentId}] Authorized: {Authorized}", paymentRequest.Id,
                        bankPaymentResponse.BankAuthorized);
                    return bankPaymentResponse;
                },
                invalidRequest =>
                {
                    logger.LogError("[Payment:{PaymentId}] Bank reports invalid request: {InvalidRequest}",
                        paymentRequest.Id,
                        invalidRequest.Message);
                    return invalidRequest;
                },
                internalError =>
                {
                    logger.LogError("[Payment:{PaymentId}] internal error: {InternalError}",
                        paymentRequest.Id,
                        internalError.Message);
                    return internalError;
                });
    }
}
