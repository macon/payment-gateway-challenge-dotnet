namespace PaymentGateway.Api.Models.Responses;

public class PaymentResponse
{
    public required string Id { get; set; }
    public required string Status { get; set; }
    public required string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    public int Amount { get; set; }
}
