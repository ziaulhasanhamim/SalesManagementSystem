namespace SalesManagementSystem.Contracts.Clients;

using System.Net;
using SalesManagementSystem.Contracts.SalesEntry;

public sealed class SalesEntriesClient
{
    private readonly HttpClient _httpClient;

    public SalesEntriesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<SalesEntryRes>> Create(CreateReq req, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/sales-entries", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var customerRes = await response.Content.ReadFromJsonAsync<SalesEntryRes>(cancellationToken: ct);
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

    public async Task<Result<IReadOnlyList<SalesEntryRes>>> GetSales(
        uint? days = null,
        CancellationToken ct = default)
    {
        DateTime? dt = days is uint val ? DateTime.Now.Date.AddDays((val - 1) * -1) : null;
        var dtUtc = dt?.ToUniversalTime();
        var response = await _httpClient.GetAsync($"/api/sales/{dtUtc:s}", ct);
        if (response.IsSuccessStatusCode)
        {
            var salesEntries = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<SalesEntryRes>>(cancellationToken: ct);
            Guard.IsNotNull(salesEntries);
            return Result.From(salesEntries);
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<SalesDataRes>> GetSalesData(
        uint? days = null,
        CancellationToken ct = default)
    {
        DateTime? dt = days is uint val ? DateTime.Now.Date.AddDays((val - 1) * -1) : null;
        var dtUtc = dt?.ToUniversalTime();
        var response = await _httpClient.GetAsync($"/api/sales-data/{dtUtc:s}", ct);
        if (response.IsSuccessStatusCode)
        {
            var salesData = await response.Content
                .ReadFromJsonAsync<SalesDataRes>(cancellationToken: ct);
            Guard.IsNotNull(salesData);
            return Result.From(salesData);
        }
        return new Error("Server didn't respond properly");
    }
}