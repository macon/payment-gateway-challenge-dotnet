using OneOf;
using PaymentGateway.App.Models.Bank.Responses;

namespace PaymentGateway.App.Infra;

/// <summary>
/// Represents the result of calling the bank's payment creation API.
///
/// Variants include:<br/>
/// <b>BankPaymentResponse</b> - A valid bank response<br/>
/// <b>InvalidRequest</b> - An invalid bank request<br/>
/// <b>TransportError</b> - A problem calling the bank API<br/>
/// <b>ResponseParsingError</b> - A problem parsing the bank's response<br/>
/// <b>UnexpectedHttpStatus</b> - An unexpected HTTP status code from the bank's API<br/>
/// </summary>
public class BankCallResult(
    OneOf<BankPaymentResponse, InvalidRequest, InternalError> input)
    : OneOfBase<BankPaymentResponse, InvalidRequest, InternalError>(input)
{
    public static implicit operator BankCallResult(BankPaymentResponse value) => new(value);
    public static implicit operator BankCallResult(InvalidRequest value) => new(value);
    public static implicit operator BankCallResult(InternalError value) => new(value);
}

public record InternalError(string Message);
public record InvalidRequest(string Message);
