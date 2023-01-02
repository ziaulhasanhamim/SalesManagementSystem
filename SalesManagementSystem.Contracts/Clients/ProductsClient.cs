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
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result> Delete(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"/api/products/{id}", ct);
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

    public async Task<Result<IReadOnlyList<ProductRes>>> GetAll(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/products", ct);
        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductRes>>(cancellationToken: ct);
            Guard.IsNotNull(products);
            return Result.From(products);
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<ProductRes>>> Search(
        string? text,
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
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<IReadOnlyList<ProductRes>>> SearchSellable(
        string? text,
        int? count = null,
        CancellationToken ct = default)
    {
        var uri = count is int val
            ? $"/api/products/search-sellables/{text}?count={val}"
            : $"/api/products/search-sellables/{text}";
        var response = await _httpClient.GetAsync(uri, ct);
        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductRes>>(cancellationToken: ct);
            Guard.IsNotNull(products);
            return Result.From(products);
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result> IncrementStock(
        StockIncrementReq req,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/products/incement-stock", req, ct);
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
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

    public async Task<Result> ChangeDeprecationState(
        ChangeDeprecationStateReq req,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/products/change-deprecation-state", req, ct);
        if (response.IsSuccessStatusCode)
        {
            return Result.Success;
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
}