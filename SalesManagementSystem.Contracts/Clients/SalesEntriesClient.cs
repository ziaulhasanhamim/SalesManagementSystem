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

    public async Task<Result<IReadOnlyList<SalesEntryRes>>> GetAll(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/sales-entries", ct);
        if (response.IsSuccessStatusCode)
        {
            var salesEntries = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<SalesEntryRes>>(cancellationToken: ct);
            Guard.IsNotNull(salesEntries);
            return Result.From(salesEntries);
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<SalesEntryRes>>> GetAllOfLastDays(
        uint days,
        CancellationToken ct = default)
    {
        var dt = DateTime.Now.Date.AddDays((days - 1) * -1)
            .ToUniversalTime();
        var response = await _httpClient.GetAsync($"/api/sales-entries/after/{dt}", ct);
        if (response.IsSuccessStatusCode)
        {
            var salesEntries = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<SalesEntryRes>>(cancellationToken: ct);
            Guard.IsNotNull(salesEntries);
            return Result.From(salesEntries);
        }
        return new Error("Server didn't respond properly");
    }
}