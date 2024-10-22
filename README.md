# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.

# My Implementation Notes
## Design considerations and assumptions

### Assumptions
From the wording of the requirements I understand that a `200` response is only to be returned from 
the `Post /payments` endpoint when a bank call returns a valid authorization outcome (`Authorized`/`Declined`).

There is no stated definition of the `Rejected` response so I've tried to use industry standards.

I do not preemptively apply bank validation rules (the valid card/payment details). 

### Considerations
Although not a stated requirement I've added some idempotency behavior on the `POST` endpoint. The implementation is as 
follows:
1. An `Idempotency-Key` HTTP header *must* be supplied by the client.
2. The header uniquely identifies a call to `POST /payments`.
3. This header is stored in a short term cache (e.g. 24h) together with a hash of the associated request payload.
4. If a duplicate request (existing key and request hash) is detected we try and fetch the original Payment.
   5. If a payment exists we return it.
   6. If not we assume we're still processing it and return a 429 advising to retry (how we represent this scenario is up for discussion).

The idempotency key store is an API layer concern so it's associated repository is located in `PaymentGateway.Api`.

I've added a new project `PaymentGateway.App` that holds all non-API layers. As the solution grows I'd likely split this
further into the usual DDD layers e.g. Api, App, Infra etc.

I like to structurally separate x-cutting concerns, so I've used the Decorator pattern to add logging to the `BankClient` 
with `BankClientLogger`. I find this approach simplifies the application logic and makes testing/mocking easier.

I've combined the `POST` call's `CancellationToken` with my own `5s` token as a way of enforcing some 
performance guarantees and deriving some SLAs/SLOs. This should be thought about more deeply, for example what 
is our behavior before and after a call has made it to the bank? This is where the async API recommendation will also 
help.

## Recommendations
Currently, `POST /payments` is synchronous and the client response is waiting on the bank call. This complicates the API 
in terms of resources and behavior. For example, if a duplicate request is detected we'd like to return the original response, 
but if the payment is still processing we can't (Currently, I return a `HTTP 429/Too many req` and we'd need to agree
and document this behavior). 

Also, what happens if the client's call times out while the bank call is still in progress? The client is left without
a payment ID but bank payment may have executed.

To remedy the above we could consider making the `POST /payments` call asynchronous as follows:
1. Make the `POST /payments` endpoint accept the request, allocate a payment ID and return it immediately with a `HTTP 202/Accepted` response.
2. Extend the payment status to also represent `Processing`.
3. The `GET /payments` endpoint works as-is but can represent `Processing`
4. If a duplicate `POST` is detected we always return the `HTTP 202/Accepted` with the original payment ID.
5. In addition, we provide a web-hook to update the client of payment processing. They can continue to use `GET /payments` if they want.

To bear in mind:
- Web-hooks come with their own set of client/server synchronisation challenges.
- The async API would now require some form of job queue.

## Preparing for production
Changes I would make before going to production:
1. Obviously flesh out the rest of the tests.
2. Extend the current skeleton observability implementation with metrics/traces etc.
3. Divide the current in-memory storage into true storage and caches.
4. In our current idempotency solution we don't differentiate clients so we could experience key clashes. Ensure there is a way to differentiate clients calls.
5. Decide the final idempotency behavior and document it.
6. For `400s`, add links (`ProblemDetails.Type`) to our real API docs.
7. Add a `Dockerfile` and other artifacts used for deployment.
8. Consume the current `Val/Opt` types as packages.

I've left various `// TODO` comments scattered around the code which we can discuss at meeting.

## Libraries used
- [Scrutor](https://github.com/khellang/Scrutor) for some extended DI features (`Decorate<T>` in my case).
- [OneOf](https://github.com/mcintyre321/OneOf) for the `OneOf<T1, Tn>` (C# discriminated union)
- The `Val<T>` & `Opt<T>` are from my own library which will soon be available publicly (also includes a `One<T, Tn>` type!)