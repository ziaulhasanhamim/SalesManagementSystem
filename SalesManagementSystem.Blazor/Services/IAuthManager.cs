using System.Security.Claims;
using SalesManagementSystem.Contracts.User;

namespace SalesManagementSystem.Blazor.Services;

public interface IAuthService
{
    public UserRes? User { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }
    bool IsAuthenticated { get; }
    Task Authorize(CancellationToken ct = default);
}