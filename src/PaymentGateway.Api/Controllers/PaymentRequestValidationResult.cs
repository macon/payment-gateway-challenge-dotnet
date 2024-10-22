using OneOf;

using PaymentGateway.App.Infra;
using PaymentGateway.App.Models.Requests;

namespace PaymentGateway.Api.Controllers;

/// <summary>
/// Represents the result of validating a POST /payment request.
///
/// Variants include:<br/>
/// <b>PaymentRequest</b> - a valid payment request<br/>
/// <b>InvalidRequest</b> - an invalid payment request<br/>
/// <b>ExistingPayment</b> - a payment request we've already handled<br/>
/// <b>StillProcessing</b> - a payment request we're already handling
/// </summary>
public class PaymentRequestValidationResult(OneOf<PaymentRequest, InvalidRequest, ExistingPayment, StillProcessing> input)
    : OneOfBase<PaymentRequest, InvalidRequest, ExistingPayment, StillProcessing>(input)
{
    public static implicit operator PaymentRequestValidationResult(InvalidRequest value) => new(value);
    public static implicit operator PaymentRequestValidationResult(PaymentRequest value) => new(value);
    public static implicit operator PaymentRequestValidationResult(ExistingPayment value) => new(value);
    public static implicit operator PaymentRequestValidationResult(StillProcessing value) => new(value);
}

public record InvalidRequest(string Message);
public record StillProcessing;
