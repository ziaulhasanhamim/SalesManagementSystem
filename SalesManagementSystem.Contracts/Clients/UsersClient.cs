namespace SalesManagementSystem.Contracts.Clients;

using System.Net;
using SalesManagementSystem.Contracts.User;

public sealed class UsersClient
{
    private readonly HttpClient _httpClient;

    public UsersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<LoginRes>> Login(LoginReq req, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/users/login", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var loginRes = await response.Content.ReadFromJsonAsync<LoginRes>(cancellationToken: ct);
            Guard.IsNotNull(loginRes);
            return loginRes;
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

    public async Task<Result<UserRes>> Authorize(string token, CancellationToken ct = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new("bearer", token);
        var response = await _httpClient.GetAsync("/api/users/authorize", ct);
        if (response.IsSuccessStatusCode)
        {
            var userRes = await response.Content.ReadFromJsonAsync<UserRes>(cancellationToken: ct);
            Guard.IsNotNull(userRes);
            return userRes;
        }
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return new UnauthorizedError();
        }
        return new Error("Server didn't respond properly");
    }

    public async Task<Result<UserRes>> Create(CreateReq req, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/users/", req, ct);
        if (response.IsSuccessStatusCode)
        {
            var createRes = await response.Content.ReadFromJsonAsync<UserRes>(cancellationToken: ct);
            Guard.IsNotNull(createRes);
            return createRes;
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

    public async Task<Result<IReadOnlyList<UserRes>>> GetAll(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/users/", ct);
        if (response.IsSuccessStatusCode)
        {
            var users = await response.Content.ReadFromJsonAsync<IReadOnlyList<UserRes>>(cancellationToken: ct);
            Guard.IsNotNull(users);
            return Result.From(users);
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