namespace PaymentGateway.App.Models.ValueTypes;

public record PaymentId(Guid Id)
{
    public static PaymentId New() => new(Guid.NewGuid());
    public static implicit operator string(PaymentId value) => value.ToString();
    public static implicit operator PaymentId(Guid id) => new(id);
    public override string ToString() => Id.ToString();
}