namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Contracts.PaymentMethod;

public static class PaymentMethodEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/payment-methods", Create).RequireAuthorization();
        app.MapGet("/api/payment-methods", GetAll).RequireAuthorization();
        app.MapGet("/api/payment-methods/{id}", Get).RequireAuthorization();
        app.MapDelete("/api/payment-methods/{id}", Delete)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
    }

    public static async ValueTask<IHttpResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var paymentMethod = req.Adapt<PaymentMethod>();
        if (await paymentMethod.IsNameDuplicate(dbContext, ct))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.Name)] = new[] { "A payment method with same name exists" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        await dbContext.AddAsync(paymentMethod, ct);
        await dbContext.SaveChangesAsync(ct);
        var res = paymentMethod.Adapt<PaymentMethodRes>();
        var uri = $"/api/payment-methods/{paymentMethod.Id}";
        return HttpResults.Created(uri, res);
    }

    public static async Task<IHttpResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var paymentMethods = await dbContext.PaymentMethods
            .ProjectToType<PaymentMethodRes>()
            .ToListAsync(ct);
        return HttpResults.Ok(paymentMethods);
    }

    public static async Task<IHttpResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var paymentMethod = await dbContext.PaymentMethods
            .Where(p => p.Id == id)
            .ProjectToType<PaymentMethodRes>()
            .FirstOrDefaultAsync(ct);
        return paymentMethod switch
        {
            not null => HttpResults.Ok(paymentMethod),
            null => HttpResults.NotFound()
        };
    }
    public static async Task<IHttpResult> Delete(
        Guid id,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var deleted = await dbContext.PaymentMethods
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);
        return deleted switch
        {
            1 => HttpResults.NoContent(),
            _ => HttpResults.NotFound()
        };
    }
}