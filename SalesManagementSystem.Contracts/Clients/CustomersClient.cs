namespace SalesManagementSystem.Contracts.Clients;

using System.Net;
using SalesManagementSystem.Contracts.Customer;

public sealed class CustomersClient
{
    private readonly HttpClient _httpClient;

    public CustomersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<CustomerRes>> Create(CreateReq req, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/customers", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var customerRes = await response.Content.ReadFromJsonAsync<CustomerRes>(cancellationToken: ct);
            Guard.IsNotNull(customerRes);
            return customerRes;
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var vError = await response.Content.ReadFromJsonAsync<ValidationErrorRes>(cancellationToken: ct);
            Guard.IsNotNull(vError);
            return vError;
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<CustomerRes>>> GetAll(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/customers", ct);
        if (response.IsSuccessStatusCode)
        {
            var customers = await response.Content.ReadFromJsonAsync<IReadOnlyList<CustomerRes>>(cancellationToken: ct);
            Guard.IsNotNull(customers);
            return Result.From(customers);
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<CustomerRes>>> SearchByName(
        string? name,
        int? count = null,
        CancellationToken ct = default)
    {
        var uri = count is int val
            ? $"/api/customers/search-name/{name}?count={val}"
            : $"/api/customers/search-name/{name}";

        var response = await _httpClient.GetAsync(uri, ct);
        Console.WriteLine($"\n\n\n\nc{await response.Content.ReadAsStringAsync()}\n\n\n\n");
        if (response.IsSuccessStatusCode)
        {
            var customers = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<CustomerRes>>(cancellationToken: ct);
            Guard.IsNotNull(customers);
            return Result.From(customers);
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<CustomerRes>>> SearchByPhoneNumber(
        string? number,
        int? count = null,
        CancellationToken ct = default)
    {
        var uri = count is int val
            ? $"/api/customers/search-number/{number}?count={val}"
            : $"/api/customers/search-number/{number}";
        var response = await _httpClient.GetAsync(uri, ct);
        if (response.IsSuccessStatusCode)
        {
            var customers = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<CustomerRes>>(cancellationToken: ct);
            Guard.IsNotNull(customers);
            return Result.From(customers);
        }
        return new Error("Server didn't respond properly");
    }
}