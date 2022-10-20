namespace SalesManagementSystem.Contracts.Clients;

using System.Net;
using SalesManagementSystem.Contracts.Product;

public sealed class ProductsClient
{
    private readonly HttpClient _httpClient;

    public ProductsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<ProductRes>> Create(CreateReq req, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/products", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var productRes = await response.Content.ReadFromJsonAsync<ProductRes>(cancellationToken: ct);
            Guard.IsNotNull(productRes);
            return productRes;
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var vError = await response.Content.ReadFromJsonAsync<ValidationErrorRes>(cancellationToken: ct);
            Guard.IsNotNull(vError);
            return vError;
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<ProductRes>>> GetAll(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/products", ct);
        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductRes>>(cancellationToken: ct);
            Guard.IsNotNull(products);
            return Result.From(products);
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<ProductRes>>> Search(
        string text,
        int? count = null,
        CancellationToken ct = default)
    {
        var uri = count is int val
            ? $"/api/products/search/{text}?count={val}"
            : $"/api/products/search/{text}";
        var response = await _httpClient.GetAsync(uri, ct);
        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductRes>>(cancellationToken: ct);
            Guard.IsNotNull(products);
            return Result.From(products);
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<ProductRes>> IncrementStock(
        StockIncrementReq req,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/products/incement-stock", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<ProductRes>(cancellationToken: ct);
            Guard.IsNotNull(products);
            return products;
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var vError = await response.Content.ReadFromJsonAsync<ValidationErrorRes>(cancellationToken: ct);
            Guard.IsNotNull(vError);
            return vError;
        }
        return new Error("Server didn't respond properly");
    }
}