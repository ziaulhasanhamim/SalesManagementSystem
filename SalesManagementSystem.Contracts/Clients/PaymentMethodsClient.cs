namespace SalesManagementSystem.Contracts.Clients;

using System.Net;
using SalesManagementSystem.Contracts.PaymentMethod;

public sealed class PaymentMethodsClient
{
    private readonly HttpClient _httpClient;

    public PaymentMethodsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<PaymentMethodRes>> Create(CreateReq req, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/payment-methods", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var payment = await response.Content.ReadFromJsonAsync<PaymentMethodRes>(cancellationToken: ct);
            Guard.IsNotNull(payment);
            return payment;
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var vError = await response.Content.ReadFromJsonAsync<ValidationErrorRes>(cancellationToken: ct);
            Guard.IsNotNull(vError);
            return vError;
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result> Delete(Guid Id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/api/payment-methods/{Id}", ct);
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFoundError("Id not Found");
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<PaymentMethodRes>>> GetAll(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/payment-methods", ct);
        if (response.IsSuccessStatusCode)
        {
            var payments = await response.Content.ReadFromJsonAsync<IReadOnlyList<PaymentMethodRes>>(cancellationToken: ct);
            Guard.IsNotNull(payments);
            return Result.From(payments);
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }
}