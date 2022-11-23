namespace SalesManagementSystem.Server;

public sealed class AppStartup
{
    public static async Task BeforeStartup(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<AppStartup>>();
        await CreateSuperAdmin(app, logger);
    }

    public static async Task CreateSuperAdmin(WebApplication app, ILogger logger)
    {
        await using var scopedService = app.Services.CreateAsyncScope();
        var authService = scopedService.ServiceProvider.GetRequiredService<IFastAuthService<User>>();
        var email = app.Configuration["SuperAdmin:Email"];
        var password = app.Configuration["SuperAdmin:Password"];
        if (email is null || password is null)
        {
            return;
        }
        var user = new User()
        {
            Email = email,
            Role = UserRoles.Admin
        };
        var authResult = await authService.CreateUser(user, password);
        if (authResult.Success)
        {
            logger.LogInformation("Super admin user created: {email}", user.Email);
            return;
        }
        if (authResult.ErrorCodes is [ FastAuthErrorCodes.DuplicateEmail ])
        {
            return;
        }
        logger.LogWarning(
            "Failed to create super admin user. Error Codes = {errorCodes}",
            string.Join(", ", authResult.ErrorCodes!));
    }
}