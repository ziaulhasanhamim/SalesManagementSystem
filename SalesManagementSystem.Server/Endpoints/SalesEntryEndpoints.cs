namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Server.ApiContracts.SalesEntry;

public static class SalesEntryEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/sales-entries", Create);
        app.MapGet("/api/sales-entries", GetAll);
        app.MapGet("/api/sales-entries/{id}", Get);
        app.MapGet("/api/sales-entries/after/{datetime}", AfterTime);
    }

    public static async ValueTask<IResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return ValidationErrorRes.BadRequest(vErrors);
        }
        var validationResult = await ValidateIds(dbContext, req, ct);
        if (validationResult.IsFailure)
        {
            return ValidationErrorRes.BadRequest((ValidationError)validationResult.Error);
        }
        var (product, _, paymentMethod) = validationResult.Value;
        if (!product.TryRemoveStock(req.Quantity))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.Quantity)] = new[] { "Specified product quantity is not in stock" }
            };
            return ValidationErrorRes.BadRequest(errors);
        }
        var salesEntry = req.Adapt<SalesEntry>();
        salesEntry.TransactionTime = DateTime.UtcNow;
        await dbContext.AddAsync(salesEntry, ct);
        await dbContext.SaveChangesAsync(ct);
        SalesEntryRes res = new(
            salesEntry.Id,
            salesEntry.ProductId,
            salesEntry.Quantity,
            salesEntry.SoldPrice,
            paymentMethod.Name,
            salesEntry.TransactionTime,
            salesEntry.CustomerId
        );
        var uri = $"/api/sales-entry/{res.Id}";
        return Results.Created(uri, res);
    }

    public static async Task<IResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var salesEntries = await dbContext.SalesEntries
            .ProjectToType<SalesEntryRes>()
            .ToListAsync(ct);
        return Results.Ok(salesEntries);
    }

    public static async Task<IResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var saleEntry = await dbContext.SalesEntries
            .Where(p => p.Id == id)
            .ProjectToType<SalesEntryRes>()
            .FirstOrDefaultAsync(ct);
        return saleEntry switch
        {
            not null => Results.Ok(saleEntry),
            null => Results.NotFound()
        };
    }

    public static async Task<IResult> AfterTime(DateTime dateTime, AppDbContext dbContext, CancellationToken ct)
    {
        var dtUtc = dateTime.ToUniversalTime();
        var salesEntries = await dbContext.SalesEntries
            .Where(p => p.TransactionTime >= dtUtc)
            .ProjectToType<SalesEntryRes>()
            .ToListAsync(ct);
        return Results.Ok(salesEntries);
    }

    private static async Task<Result<(Product, Customer?, PaymentMethod)>> ValidateIds(
        AppDbContext dbContext, CreateReq req, CancellationToken ct)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == req.ProductId, ct);
        if (product is null)
        {
            var errorInfos = new FieldErrorInfo[]
            {
                new(nameof(CreateReq.ProductId), "Product id is invalid")
            };
            return new ValidationError("Provided request data was not valid", errorInfos);
        }
        var customer = req.CustomerId switch
        {
            { } customerId => await dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId, ct),
            _ => null
        };
        if (customer is null && req.CustomerId is not null)
        {
            var errorInfos = new FieldErrorInfo[]
            {
                new(nameof(CreateReq.CustomerId), "Customer id is invalid")
            };
            return new ValidationError("Provided request data was not valid", errorInfos);
        }
        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == req.PaymentMethodId, ct);
        if (paymentMethod is null)
        {
            var errorInfos = new FieldErrorInfo[]
            {
                new(nameof(CreateReq.CustomerId), "Payment method id is invalid")
            };
            return new ValidationError("Provided request data was not valid", errorInfos);
        }
        return (product, customer, paymentMethod);
    }
}