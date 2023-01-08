namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Contracts.User;


public static class UserEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/users/login/", Login);
        app.MapGet("/api/users/authorize/", Authorize)
            .RequireAuthorization();
        app.MapPost("/api/users/", Create)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
        app.MapGet("/api/users/", GetAll)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
        app.MapDelete("/api/users/{id}", Delete)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
    }

    public static async Task<IHttpResult> Login(
        IFastAuthService<User> authService,
        LoginReq req,
        CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var result = await authService.Authenticate(req.Email, req.Password, ct);
        if (result is not AuthResult<User>.Success { AccessToken: var token, AccessTokenExpiresAt: var exp })
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                ["User"] = new[] { "Provided credentials were wrong" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        return HttpResults.Ok(new LoginRes(token, exp));
    }

    public static async Task<IHttpResult> Create(
        IFastAuthService<User> authService,
        CreateReq req,
        CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var role = req.Role.ToLower();
        if (role is not (UserRoles.Seller or UserRoles.Admin))
        {
            Dictionary<string, IEnumerable<string>> errs = new()
            {
                [nameof(req.Role)] = new[] { $"Role must be {UserRoles.Admin} or {UserRoles.Seller}" }
            };
            return HttpHelpers.BadRequest(errs);
        }
        User user = new()
        {
            Email = req.Email,
            Role = role
        };
        var result = await authService.CreateUser(user, req.Password, ct);
        if (result.Success)
        {
            return HttpResults.Ok(user.Adapt<UserRes>());
        }
        var errors = mapErrors(result);
        return HttpHelpers.BadRequest(errors);
    }

    public static async Task<IHttpResult> GetAll(
        AppDbContext dbCtx,
        CancellationToken ct)
    {
        var users = await dbCtx.Users
            .ProjectToType<UserRes>()
            .ToListAsync(ct);
        return HttpResults.Ok(users);
    }

    private static Dictionary<string, IEnumerable<string>> mapErrors(CreateUserResult result)
    {
        Dictionary<string, IEnumerable<string>> errors = new();
        foreach (var err in result.ErrorCodes!)
        {
            switch (err)
            {
                case FastAuthErrorCodes.DuplicateEmail:
                    setErr(nameof(User.Email), "Email already eixts");
                    break;
                case FastAuthErrorCodes.InvalidEmailFormat:
                    setErr(nameof(User.Email), "Incorrent Email");
                    break;
            }
        }
        return errors;

        void setErr(string key, string msg)
        {
            if (errors.TryGetValue(key, out var errList))
            {
                ((List<string>)errList).Add(msg);
                return;
            }
            var errList2 = new List<string>()
            {
                msg
            };
            errors.Add(key, errList2);
        }
    }

    public static async Task<IHttpResult> Delete(
        Guid id,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var deleted = await dbContext.Users
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);
        return deleted switch
        {
            1 => HttpResults.NoContent(),
            _ => HttpResults.NotFound()
        };
    }

    public static IHttpResult Authorize(HttpContext ctx)
    {
        var user = ctx.User.GetUser();
        return HttpResults.Ok(user.Adapt<UserRes>());
    }
}