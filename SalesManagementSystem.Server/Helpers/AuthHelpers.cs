namespace SalesManagementSystem.Server.Helpers;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

static class AuthHelpers
{
    public static User GetUser(this ClaimsPrincipal principal)
    {
        var user = new User();
        foreach (var claim in principal.Claims)
        {
            switch (claim.Type)
            {
                case JwtRegisteredClaimNames.Email:
                    user.Email = claim.Value;
                    break;
                case ClaimTypes.Role:
                    user.Role = claim.Value;
                    break;
            }
        }
        return user;
    }
}