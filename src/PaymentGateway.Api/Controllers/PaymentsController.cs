using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Infra;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.App.Models.Domain;
using PaymentGateway.App.Models.Requests;
using PaymentGateway.App.Models.ValueTypes;
using PaymentGateway.App.Services;
using PaymentGateway.Lib.Functional;

namespace PaymentGateway.Api.Controllers;

using PaymentResult = ActionResult<PaymentResponse?>;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(PaymentRequestCache paymentRequestCache, PaymentService paymentService)
    : Controller
{
    [HttpGet("{id}")]
    public PaymentResult GetPaymentAsync(string id) =>
        (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var paymentId))
            ? BadRequest($"Supplied payment Id '{id}' is invalid. Must be a valid GUID.")
            : paymentService.GetPayment(paymentId)
                .Match(
                    none: () => NotFound($"Payment not found: {id}"),
                    some: OkResult);

    [HttpPost]
    public async Task<PaymentResult> PostPaymentAsync(
        [FromBody] PostPaymentRequest? postPaymentRequest, 
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        var requestValidity = ValidatePaymentRequest(idempotencyKey, postPaymentRequest);

        return await
            requestValidity.Match(
                paymentRequest => ProcessPaymentRequest(paymentRequest),
                invalidRequest => Task.FromResult(BadRequest(invalidRequest.Message)),
                existingPayment => Task.FromResult(OkResult(existingPayment.Payment)),
                // TODO: document use of 429 for idempotency.
                _ => Task.FromResult(TryAgain("Too many payment requests. Try again later.")));
    }

    // TODO: This isn't ideal but does the job of clearing the payment request cache of any requests that have failed at the bank.
    private Task<PaymentResult> ProcessPaymentRequest(PaymentRequest paymentRequest) 
        =>
        paymentService.MakePayment(paymentRequest, GetToken)
            .Match(
                payment => OkResult(payment),
                badBankRequest =>
                {
                    paymentRequestCache.Remove(paymentRequest);
                    return BadRequest(badBankRequest.Message);
                },
                internalError =>
                {
                    paymentRequestCache.Remove(paymentRequest);
                    return Problem(internalError.Message);
                });

    private PaymentRequestValidationResult ValidatePaymentRequest(string? idempotencyKey, PostPaymentRequest? postPaymentRequest)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return new InvalidRequest("Missing header: Idempotency-Key");
        }

        var paymentRequestVal =
            postPaymentRequest == null
                ? Val<PaymentRequest>.Invalid("Malformed request")
                : PaymentRequest.New(
                    CreditCard.New(postPaymentRequest.CardNumber, postPaymentRequest.ExpiryMonth,
                        postPaymentRequest.ExpiryYear, postPaymentRequest.Cvv),
                    Amount.New(postPaymentRequest.Amount, postPaymentRequest.Currency),
                    idempotencyKey,
                    postPaymentRequest.GetHash);

        return paymentRequestVal.Match<PaymentRequestValidationResult>(
            err => new InvalidRequest(err),
            paymentRequest =>
            {
                if (paymentRequestCache.TryAddRequest(paymentRequest, out var existingRequest))
                {
                    return paymentRequest;
                }

                if (!existingRequest.RequestHash.SequenceEqual(paymentRequest.RequestHash))
                {
                    return new InvalidRequest($"Reuse of Idempotency-Key: {paymentRequest.IdempotencyKey} within 24h period.");
                }

                return paymentService.GetPaymentByRequest(existingRequest.Id)
                    .Match(
                        () => new StillProcessing(),
                        existingPayment => new PaymentRequestValidationResult(existingPayment));
            });
    }

    private CancellationToken GetToken =>
        CancellationTokenSource
            .CreateLinkedTokenSource(
                HttpContext.RequestAborted,
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token)
            .Token;

    private PaymentResult BadRequest(string error) =>
        BadRequest(new ProblemDetails
        {
            Status = 400,
            Title = "One or more validation errors occurred",
            Detail = error,
            Instance = HttpContext.TraceIdentifier,
            Type = "https://github.com/cko-recruitment"
        });
    
    private PaymentResult NotFound(string error) =>
        NotFound(new ProblemDetails
        {
            Status = 404,
            Title = "There was a problem with your request",
            Detail = error,
            Instance = HttpContext.TraceIdentifier
        });

    private PaymentResult Problem(string error) =>
        new ObjectResult(new ProblemDetails
        {
            Status = 500,
            Title = "There was a problem processing your request",
            Detail = error,
            Instance = HttpContext.TraceIdentifier,
            Type = "https://github.com/cko-recruitment"
        }) { StatusCode = 500 };

    private PaymentResult TryAgain(string error) =>
        new ObjectResult(new ProblemDetails
        {
            Status = 429,
            Title = "Too many requests",
            Detail = error,
            Instance = HttpContext.TraceIdentifier,
            Type = "https://github.com/cko-recruitment"
        }) { StatusCode = 429 };

    private PaymentResult OkResult(Payment payment) =>
        Ok(payment.ToPostPaymentResponse());
}
