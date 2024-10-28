using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentGateway.App.Models.Bank.Responses;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class BankPaymentResponse
{
    [JsonConstructor]
    public BankPaymentResponse(bool? authorized, string? authorizationCode)
    {
        if (authorized == null)
        {
            throw new JsonException("Missing required element 'authorized:bool'");
        }
        
        if (authorized.Value)
        {
            ArgumentNullException.ThrowIfNull(authorizationCode,
                "A successful bank authorization must contain 'authorization_code:string'");
        }
        this.Authorized = authorized.Value;
        this.AuthorizationCode = authorizationCode;
    }

    [JsonInclude]
    private bool? Authorized { get; }
    public string? AuthorizationCode { get; }

    // TODO: This is here as a result of the ctor rules. I want to ensure that 'authorized' is always present and, if true, 'authorization_code' is present.
    // System.Text.Json has some rigid rules around deserializing with constructors.
    [JsonIgnore]
    public bool BankAuthorized => Authorized!.Value;
}