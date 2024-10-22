using System.Diagnostics.CodeAnalysis;

using PaymentGateway.App.Models.Requests;

namespace PaymentGateway.Api.Infra;

// TODO: document payment request cache and its TTL etc

/// <summary>
/// This is a short-term cache of payment requests.
/// It enables some idempotency on the POST /payments endpoint protecting against network issues which lead to retries.
/// It should have a fairly short TTL (e.g. 24h).
/// The usage of the Idempotency-Key HTTP header and cache TTL should be in the public API docs.
/// </summary>
public class PaymentRequestCache
{
    private readonly Dictionary<string, PaymentRequest> _cache = new();

    public bool Remove(PaymentRequest paymentRequest) => _cache.Remove(paymentRequest.IdempotencyKey);

    public bool TryAddRequest(PaymentRequest paymentRequest, [NotNullWhen(false)] out PaymentRequest? existingRequest)
    {
        if (!_cache.TryAdd(paymentRequest.IdempotencyKey, paymentRequest))
        {
            existingRequest = _cache[paymentRequest.IdempotencyKey];
            return false;
        }

        existingRequest = null;
        return true;
    }
}