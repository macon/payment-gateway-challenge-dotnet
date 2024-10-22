using FluentAssertions;

using Moq;
using PaymentGateway.App.Infra;
using PaymentGateway.App.Models.Bank.Responses;
using PaymentGateway.App.Models.Domain;
using PaymentGateway.App.Models.Requests;
using PaymentGateway.App.Services;
using PaymentGateway.App.Tests.Shared;

using InternalError = PaymentGateway.App.Infra.InternalError;

namespace PaymentGateway.App.Tests;

public class PaymentServiceTests
{

    public class SendPaymentTests
    {
        private readonly Mock<IBankClient> _mockBankClient = new();
        
        [Fact]
        public async Task Handles_Payment_Creation()
        {
            _mockBankClient
                .Setup(x => x.SendPayment(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BankCallResult(new BankPaymentResponse(true, PaymentBuilder.AuthorizationCode)));
            
            var paymentRequest = PaymentBuilder.GetPaymentRequest();
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, _mockBankClient.Object);
            
            // Act
            var paymentResult = await paymentService.MakePayment(paymentRequest, CancellationToken.None);
            
            // Assert
            paymentResult.Should().NotBeNull();
            var payment = paymentResult.Value.Should().BeOfType<Payment>().Subject;

            // Check the payment repository has been updated.
            paymentRepository.GetPaymentFromRequest(paymentRequest.Id)
                .Match(
                    () => throw new Exception(),
                    existingPayment => existingPayment.Payment.Id.Should().Be(payment.Id));
            
            paymentRepository.Get(payment.Id).Match(
                    () => throw new Exception(),
                    p =>
                    {
                        p.Id.Should().Be(payment.Id);
                        p.Status.Should().Be(payment.Status);
                        return true;
                    });
        }
        
        [Fact]
        public async Task Handles_BadRequest()
        {
            _mockBankClient
                .Setup(x => x.SendPayment(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BankCallResult(new InvalidRequest("Some secret internal error message")));
            
            var paymentRequest = PaymentBuilder.GetPaymentRequest();
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, _mockBankClient.Object);
            
            // Act
            var paymentResult = await paymentService.MakePayment(paymentRequest, CancellationToken.None);
            
            // Assert
            paymentResult.Should().NotBeNull();
            var invalidRequest = paymentResult.Value.Should().BeOfType<BadRequest>().Subject;
            invalidRequest.Message.Should().Be("Invalid card details.");

            // Check the payment repository has not been updated.
            paymentRepository.GetPaymentFromRequest(paymentRequest.Id).IsNone.Should().BeTrue();
        }
        
        [Fact]
        public async Task Handles_InternalError()
        {
            _mockBankClient
                .Setup(x => x.SendPayment(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BankCallResult(new InternalError("Some secret internal error message")));
            
            var paymentRequest = PaymentBuilder.GetPaymentRequest();
            var paymentRepository = new PaymentsRepository();
            var paymentService = new PaymentService(paymentRepository, _mockBankClient.Object);
            
            // Act
            var paymentResult = await paymentService.MakePayment(paymentRequest, CancellationToken.None);
            
            // Assert
            paymentResult.Should().NotBeNull();
            var internalError = paymentResult.Value.Should().BeOfType<PaymentGateway.App.Services.InternalError>().Subject;
            internalError.Message.Should().Be("An error occurred processing your payment.");

            // Check the payment repository has not been updated.
            paymentRepository.GetPaymentFromRequest(paymentRequest.Id).IsNone.Should().BeTrue();
        }
    }
}