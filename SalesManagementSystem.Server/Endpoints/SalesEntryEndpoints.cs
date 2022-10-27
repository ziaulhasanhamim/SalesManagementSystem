namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Contracts.SalesEntry;

public static class SalesEntryEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/sales-entries", Create);
        app.MapGet("/api/sales-entries", GetAll);
        app.MapGet("/api/sales-entries/{id}", Get);
        app.MapGet("/api/sales-entries/after/{datetime}", AfterTime);
    }

    public static async ValueTask<IHttpResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var validationResult = await ValidateIds(dbContext, req, ct);
        if (validationResult.IsFailure)
        {
            return HttpResults.BadRequest((ValidationErrorRes)validationResult.Error);
        }
        var (product, customer, paymentMethod) = validationResult.Value;
        if (!product.TryRemoveStock(req.Quantity))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.Quantity)] = new[] { "Specified product quantity is not in stock" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        var salesEntry = req.Adapt<SalesEntry>();
        salesEntry.SetTransactionTime();
        await dbContext.AddAsync(salesEntry, ct);
        await dbContext.SaveChangesAsync(ct);
        SalesEntryRes res = new(
            salesEntry.Id,
            product.Name,
            salesEntry.Quantity,
            salesEntry.SoldPrice,
            product.BuyingPrice,
            paymentMethod.Name,
            salesEntry.TransactionTime,
            customer?.PhoneNumber,
            customer?.Name
        );
        var uri = $"/api/sales-entry/{res.Id}";
        return HttpResults.Created(uri, res);
    }

    public static async Task<IHttpResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var salesEntries = await dbContext.SalesEntries
            .OrderByDescending(s => s.TransactionTime)
            .Select(s => new SalesEntryRes(
                s.Id,
                s.Product!.Name,
                s.Quantity,
                s.SoldPrice,
                s.Product.BuyingPrice,
                s.PaymentMethod!.Name,
                s.TransactionTime,
                s.Customer!.PhoneNumber,
                s.Customer!.Name
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(salesEntries);
    }

    public static async Task<IHttpResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var saleEntry = await dbContext.SalesEntries
            .Where(p => p.Id == id)
            .Select(s => new SalesEntryRes(
                s.Id,
                s.Product!.Name,
                s.Quantity,
                s.SoldPrice,
                s.Product.BuyingPrice,
                s.PaymentMethod!.Name,
                s.TransactionTime,
                s.Customer!.PhoneNumber,
                s.Customer!.Name
            ))
            .FirstOrDefaultAsync(ct);
        return saleEntry switch
        {
            not null => HttpResults.Ok(saleEntry),
            null => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> AfterTime(DateTime dateTime, AppDbContext dbContext, CancellationToken ct)
    {
        var dtUtc = dateTime.ToUniversalTime();
        var salesEntries = await dbContext.SalesEntries
            .Where(p => p.TransactionTime >= dtUtc)
            .OrderByDescending(s => s.TransactionTime)
            .Select(s => new SalesEntryRes(
                s.Id,
                s.Product!.Name,
                s.Quantity,
                s.SoldPrice,
                s.Product.BuyingPrice,
                s.PaymentMethod!.Name,
                s.TransactionTime,
                s.Customer!.PhoneNumber,
                s.Customer!.Name
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(salesEntries);
    }

    private static async Task<Result<(Product, Customer?, PaymentMethod)>> ValidateIds(
        AppDbContext dbContext, CreateReq req, CancellationToken ct)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == req.ProductId, ct);
        if (product is null)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.ProductId)] = new[] { "Product id is invalid" }
            };
            return new ValidationErrorRes(errors);
        }
        var customer = req.CustomerId switch
        {
            Guid customerId => await dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId, ct),
            _ => null
        };
        if (customer is null && req.CustomerId is not null)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.CustomerId)] = new[] { "Customer id is invalid" }
            };
            return new ValidationErrorRes(errors);
        }
        var paymentMethod = await dbContext.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == req.PaymentMethodId, ct);
        if (paymentMethod is null)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.CustomerId)] = new[] { "Payment method id is invalid" }
            };
            return new ValidationErrorRes(errors);
        }
        return (product, customer, paymentMethod);
    }
}