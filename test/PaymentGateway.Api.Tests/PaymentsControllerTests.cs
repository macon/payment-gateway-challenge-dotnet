using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.App.Infra;
using PaymentGateway.App.Models;
using PaymentGateway.App.Models.Bank;
using PaymentGateway.App.Models.Bank.Responses;
using PaymentGateway.App.Models.Domain;
using PaymentGateway.App.Models.Requests;
using PaymentGateway.App.Models.ValueTypes;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    
    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = GeneratePayment();
        
        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.AddPayment(payment);
        
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder
                    .ConfigureServices(services => ((ServiceCollection)services)
                        .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(payment.Id, paymentResponse!.Id);
        // TODO: assert remaining properties...
    }

    [Fact]
    public async Task PostsPaymentSuccessfully()
    {
        // Arrange
        var mockBankClient = new Mock<IBankClient>();
        mockBankClient
            .Setup(x => x.SendPayment(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BankCallResult(new BankPaymentResponse(true, "auth_code")));
        
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder
                    .ConfigureTestServices(services
                        => services.AddScoped<IBankClient>(_ => mockBankClient.Object)))
            .CreateClient();

        var paymentRequest = TestPaymentRequest;
        var httpContent = new StringContent(
            JsonSerializer.Serialize(paymentRequest, JsonOptions), 
            Encoding.UTF8, 
            "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/Payments") { Content = httpContent };
        httpRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        
        // Act
        var httpResponse = await client.SendAsync(httpRequest);
        var paymentResponse = await httpResponse.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentRequest.Amount, paymentResponse!.Amount);
        Assert.Equal(paymentRequest.CardNumber[^4..], paymentResponse!.CardNumberLastFour);
        // TODO: assert remaining properties...
    }

    
    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    private Payment GeneratePayment()
    {
        var bankResponse = new BankPaymentResponse(true, Guid.NewGuid().ToString());
        
        var creditCard = CreditCard.New(
            cardNumber: "2222405343248877",
            expiryMonth: _random.Next(1, 12),
            expiryYear: _random.Next(DateTime.UtcNow.Year, 2030),
            cvv: "123",
            ((_, _) => false));
        
        var paymentAmount = Amount.New(amount: _random.Next(1, 10000), currencyCode: "GBP");

        var paymentRequest = PaymentRequest.New(
            creditCard,
            paymentAmount,
            Guid.NewGuid().ToString(),
            TestPaymentRequest.GetHash);

        var payment = paymentRequest
            .Map(request => Payment.New(request, bankResponse))
            .Match(
                err => throw new TestClassException($"Payment test data setup failed: {err}"),
                p => p);
        
        return payment;
    }
    
    private PostPaymentRequest TestPaymentRequest => new()
    {
        CardNumber = "2222405343248877",
        ExpiryMonth = _random.Next(1, 12),
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = _random.Next(1, 10000),
        Cvv = "123"
    };
}
