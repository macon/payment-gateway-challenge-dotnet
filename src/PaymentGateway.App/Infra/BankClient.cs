using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using PaymentGateway.App.Models.Bank.Requests;
using PaymentGateway.App.Models.Bank.Responses;
using PaymentGateway.App.Models.Requests;

namespace PaymentGateway.App.Infra;

public interface IBankClient
{
    Task<BankCallResult> SendPayment(PaymentRequest paymentRequest, CancellationToken cancellationToken);
}

public class BankClient(HttpClient httpClient) : IBankClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<BankCallResult> SendPayment(PaymentRequest paymentRequest, CancellationToken cancellationToken)
    {
        var httpContent = new StringContent(
            JsonSerializer.Serialize(paymentRequest.ToBankPaymentRequest(), JsonSerializerOptions), 
            Encoding.UTF8, 
            "application/json");

        try
        {
            using var httpResponseMessage = await httpClient.PostAsync("payments", httpContent, cancellationToken);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new InvalidRequest(await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken));
                }

                return new InternalError($"Unexpected bank HTTP status code: {httpResponseMessage.StatusCode}");
            }

            var bankPaymentResponse =
                await httpResponseMessage.Content.ReadFromJsonAsync<BankPaymentResponse>(JsonSerializerOptions,
                    cancellationToken);

            return bankPaymentResponse == null
                ? new InternalError("Problem parsing bank payment response")
                : bankPaymentResponse;
        }
        catch (Exception e)
        {
            return new InternalError(e.Message);
        }
    }
}