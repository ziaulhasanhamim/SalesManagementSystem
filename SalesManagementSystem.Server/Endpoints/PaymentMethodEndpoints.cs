namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Server.ApiContracts.PaymentMethods;

public static class PaymentMethodEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/payment-methods", Create);
        app.MapGet("/api/payment-methods", GetAll);
        app.MapGet("/api/payment-methods/{id}", Get);
    }

    public static async ValueTask<IResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return ValidationErrorRes.BadRequest(vErrors);
        }
        var paymentMethod = req.Adapt<PaymentMethod>();
        if (await paymentMethod.IsNameDuplicate(dbContext, ct))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.Name)] = new[] { "A payment method with same name exists" }
            };
            return ValidationErrorRes.BadRequest(errors);
        }
        await dbContext.AddAsync(paymentMethod, ct);
        await dbContext.SaveChangesAsync(ct);
        var res = paymentMethod.Adapt<PaymentMethodRes>();
        var uri = $"/api/payment-methods/{paymentMethod.Id}";
        return Results.Created(uri, res);
    }

    public static async Task<IResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var paymentMethods = await dbContext.PaymentMethods
            .ProjectToType<PaymentMethodRes>()
            .ToListAsync(ct);
        return Results.Ok(paymentMethods);
    }

    public static async Task<IResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var paymentMethod = await dbContext.PaymentMethods
            .Where(p => p.Id == id)
            .ProjectToType<PaymentMethodRes>()
            .FirstOrDefaultAsync(ct);
        return paymentMethod switch
        {
            not null => Results.Ok(paymentMethod),
            null => Results.NotFound()
        };
    }
}