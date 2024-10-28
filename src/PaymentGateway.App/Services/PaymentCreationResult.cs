using OneOf;

using PaymentGateway.App.Models.Domain;

namespace PaymentGateway.App.Services;

/// <summary>
/// Represents the result of calling the App's payment creation API.
///
/// Variants include:<br/>
/// <b>Payment</b> - A new Payment<br/>
/// <b>BadRequest</b> - An invalid App payment request<br/>
/// <b>InternalError</b> - An internal error occurred<br/>
/// </summary>
public class PaymentCreationResult(OneOf<Payment, BadRequest, InternalError> input)
    : OneOfBase<Payment, BadRequest, InternalError>(input)
{
    public static implicit operator PaymentCreationResult(Payment value) => new(value);
    public static implicit operator PaymentCreationResult(BadRequest value) => new(value);
    public static implicit operator PaymentCreationResult(InternalError value) => new(value);
}

public record InternalError(string Message);
public record BadRequest(string Message);

public static class PaymentCreationResultExt
{
    public static async Task<R> Match<R>(
        this Task<PaymentCreationResult> result, 
        Func<Payment, R> f0, 
        Func<BadRequest, R> f1,
        Func<InternalError, R> f2)
    {
        return (await result).Match(f0, f1, f2);
    }
}
