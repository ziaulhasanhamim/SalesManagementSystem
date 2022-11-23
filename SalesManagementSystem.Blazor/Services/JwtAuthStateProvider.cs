namespace SalesManagementSystem.Blazor.Services;

using System.Security.Claims;
using SalesManagementSystem.Contracts.User;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider, IAuthService
{
    readonly UsersClient _usersClient;
    readonly IJSRuntime _jSRuntime;
    ClaimsPrincipal? _claimsPrincipal;
    UserRes? _user;
    bool _initialized;

    public JwtAuthStateProvider(UsersClient usersClient, IJSRuntime jSRuntime)
    {
        _usersClient = usersClient;
        _jSRuntime = jSRuntime;
    }

    public UserRes? User
    {
        get => _user;
        private set
        {
            if (value is null)
            {
                _user = null;
                _claimsPrincipal = null;
                return;
            }
            _user = value;
            SetClaims(_user);
        }
    }

    public bool IsAuthenticated => User is null;

    public ClaimsPrincipal ClaimsPrincipal
    {
        get => _claimsPrincipal ?? new(new ClaimsIdentity());
        private set => _claimsPrincipal = value;
    }

    public async Task Authorize(CancellationToken ct = default)
    {
        await checkIfUserAuthenticated(ct);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private void SetClaims(UserRes user)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, "bearer");
        ClaimsPrincipal = new(identity);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            await checkIfUserAuthenticated();
        }
        return new AuthenticationState(ClaimsPrincipal);
    }

    async Task checkIfUserAuthenticated(CancellationToken ct = default)
    {
        var token = await _jSRuntime.InvokeAsync<string?>("localStorage.getItem", "auth");
        if (string.IsNullOrEmpty(token))
        {
            User = null;
            return;
        }
        var apiResult = await _usersClient.Authorize(token, ct);
        if (apiResult.IsFailure)
        {
            User = null;
            if (apiResult.Error is UnauthorizedError)
            {
                return;
            }
            throw new Exception(apiResult.Error.Message);
        }
        User = apiResult.Value;
        _initialized = true;
    }
}