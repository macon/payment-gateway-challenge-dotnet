// ReSharper disable InconsistentNaming

using PaymentGateway.App.Models.ValueTypes;
using PaymentGateway.Lib.Functional;

namespace PaymentGateway.App.Models.Requests;

public record PaymentRequest
{
    public PaymentRequestId Id { get; }
    public CreditCard CreditCard { get; }
    public Amount Amount { get; }
    public DateTime CreatedAt { get; }
    public string IdempotencyKey { get; }
    public byte[] RequestHash { get; }
    
    public PaymentRequest(CreditCard creditCard, Amount amount, string idempotencyKey, byte[] requestHash)
    {
        Id = PaymentRequestId.New();
        CreditCard = creditCard;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        IdempotencyKey = idempotencyKey;
        RequestHash = requestHash;
    }

    public static Val<PaymentRequest> New(
        Val<CreditCard> creditCard,
        Val<Amount> amount,
        string idempotencyKey,
        byte[] requestHash)
        =>
            creditCard
                .Combine(amount,
                    (card, txAmount) => new PaymentRequest(card, txAmount, idempotencyKey, requestHash));
}
