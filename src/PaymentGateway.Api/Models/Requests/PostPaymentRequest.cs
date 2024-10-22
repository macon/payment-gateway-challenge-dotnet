using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest
{
    public required string CardNumber { get; init; }
    
    public int ExpiryMonth { get; init; }
    
    public int ExpiryYear { get; init; }
    
    public required string Currency { get; init; }
    
    public int Amount { get; init; }
    
    public required string Cvv { get; init; }

    public override string ToString() => $"{CardNumber}-{Amount}-{ExpiryMonth}-{ExpiryYear}-{Currency}-{Cvv}";
    
    [JsonIgnore]
    public byte[] GetHash => SHA256.HashData(Encoding.UTF8.GetBytes(ToString()));
}