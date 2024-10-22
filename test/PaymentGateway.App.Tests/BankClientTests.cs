using System.Net;

using FluentAssertions;
using PaymentGateway.App.Infra;
using PaymentGateway.App.Models.Bank.Responses;
using PaymentGateway.App.Tests.Shared;
using RichardSzalay.MockHttp;

namespace PaymentGateway.App.Tests;

public class BankClientTests
{
    public class SendPaymentTests
    {
        private readonly MockHttpMessageHandler _mockHandler = new();

        [Fact]
        public async Task Handles_Authorised_Response()
        {
            const string authorizationCode = "0bb07405-6d44-4b50-a14f-7ae0beff13ad";
            var httpClient = AuthorizingClient(authorizationCode);
            var sut = new BankClient(httpClient);
            var paymentRequest = PaymentBuilder.GetPaymentRequest();

            // Act
            var response = await sut.SendPayment(paymentRequest, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            var bankPaymentResponse = response.Value.Should().BeOfType<BankPaymentResponse>().Subject;
            bankPaymentResponse.BankAuthorized.Should().BeTrue();
            bankPaymentResponse.AuthorizationCode.Should().Be(authorizationCode);
        }

        [Fact]
        public async Task Handles_Declined_Response()
        {
            var httpClient = DecliningClient();
            var sut = new BankClient(httpClient);
            var paymentRequest = PaymentBuilder.GetPaymentRequest();

            // Act
            var response = await sut.SendPayment(paymentRequest, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            var bankPaymentResponse = response.Value.Should().BeOfType<BankPaymentResponse>().Subject;
            bankPaymentResponse.BankAuthorized.Should().BeFalse();
            bankPaymentResponse.AuthorizationCode.Should().BeNull();
        }

        [Fact]
        public async Task Handles_InvalidRequest_Response()
        {
            var httpClient = BadRequestClient();
            var sut = new BankClient(httpClient);
            var paymentRequest = PaymentBuilder.GetPaymentRequest();

            // Act
            var response = await sut.SendPayment(paymentRequest, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Value.Should().BeOfType<InvalidRequest>();
        }

        [Fact]
        public async Task Handles_InternalError_Response()
        {
            var httpClient = InternalErrorClient();
            var sut = new BankClient(httpClient);
            var paymentRequest = PaymentBuilder.GetPaymentRequest();

            // Act
            var response = await sut.SendPayment(paymentRequest, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Value.Should().BeOfType<InternalError>();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK, "{}")]
        [InlineData(HttpStatusCode.OK, "{\"authorized\": true}")]
        [InlineData(HttpStatusCode.OK, "{\"auth\": true}")]
        [InlineData(HttpStatusCode.OK, "{\"authorization_code\": \"auth_xyz\"}")]
        public async Task Handles_Unrecognised_Response_Body(HttpStatusCode httpStatusCode, string content)
        {
            var httpClient = HttpResponse(httpStatusCode, content);
            var sut = new BankClient(httpClient);
            var paymentRequest = PaymentBuilder.GetPaymentRequest();

            // Act
            var response = await sut.SendPayment(paymentRequest, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();
            response.Value.Should().BeOfType<InternalError>();
        }

        private HttpClient AuthorizingClient(string authorizationCode) => 
            HttpResponse(HttpStatusCode.OK, $"{{\"authorized\": true, \"authorization_code\": \"{authorizationCode}\"}}");

        private HttpClient DecliningClient() => HttpResponse(HttpStatusCode.OK, "{\"authorized\": false}");

        private HttpClient BadRequestClient() => 
            HttpResponse(
                HttpStatusCode.BadRequest, 
                "{\"errorMessage\": \"The request supplied is not supported by the simulator\"}");

        private HttpClient InternalErrorClient() => HttpResponse(HttpStatusCode.GatewayTimeout, "{\"errorMessage\": \"Boom!\"}");

        private HttpClient HttpResponse(HttpStatusCode httpStatusCode, string jsonContent)
        {
            _mockHandler
                .When(HttpMethod.Post, "http://gateway.cko.com/api/payments")
                .Respond(
                    httpStatusCode,
                    "application/json",
                    jsonContent);

            var client = _mockHandler.ToHttpClient();
            client.BaseAddress = new Uri("http://gateway.cko.com/api/");
            return client;
        }
    }
}