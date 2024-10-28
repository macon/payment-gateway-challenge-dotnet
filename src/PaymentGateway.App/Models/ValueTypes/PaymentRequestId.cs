namespace PaymentGateway.App.Models.ValueTypes;

public record PaymentRequestId(Guid Id)
{
    public static PaymentRequestId New() => new(Guid.NewGuid());
}