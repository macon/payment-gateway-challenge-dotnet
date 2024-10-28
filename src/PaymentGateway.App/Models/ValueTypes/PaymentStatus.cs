namespace PaymentGateway.App.Models.ValueTypes;

public sealed record PaymentStatus
{
    private const string AuthorizedValue = "Authorized";
    private const string DeclinedValue = "Declined";
    
    public string Value { get; }

    public PaymentStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || (value != AuthorizedValue && value != DeclinedValue))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value);
        }

        Value = value;
    }

    public static PaymentStatus Authorized => new(AuthorizedValue);
    public static PaymentStatus Declined => new(DeclinedValue);
    public override string ToString() => Value;
    public static implicit operator string(PaymentStatus value) => value.Value;
}