namespace PaymentGateway.App.Models.Bank.Requests;

public class BankPaymentRequest
{
    public required string CardNumber { get; init; }
    public required string ExpiryDate { get; init; }
    public required string Currency { get; init; }
    public int Amount { get; init; }
    public required string Cvv { get; init; }
}