namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Server.ApiContracts.Product;

public static class ProductEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/products", Create);
        app.MapGet("/api/products", GetAll);
        app.MapGet("/api/products/{id}", Get);
        app.MapGet("/api/products/search/{text}", Search);
        app.MapPost("/api/products/incement-stock", IncrementStock);
    }

    public static async ValueTask<IResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return ValidationErrorRes.BadRequest(vErrors);
        }
        if (req.SellingPrice < req.BuyingPrice)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.SellingPrice)] = new[] { "Selling price must be greater than buying price" }
            };
            return ValidationErrorRes.BadRequest(errors);
        }
        var product = req.Adapt<Product>();
        if (await product.IsNameDuplicate(dbContext, ct))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.Name)] = new[] { "A product with same name exists" }
            };
            return ValidationErrorRes.BadRequest(errors);
        }
        await dbContext.AddAsync(product, ct);
        await dbContext.SaveChangesAsync(ct);
        var res = product.Adapt<ProductRes>();
        var uri = $"/api/products/{product.Id}";
        return Results.Created(uri, res);
    }

    public static async Task<IResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var products = await dbContext.Products
            .ProjectToType<ProductRes>()
            .ToListAsync(ct);
        return Results.Ok(products);
    }

    public static async Task<IResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Where(p => p.Id == id)
            .ProjectToType<ProductRes>()
            .FirstOrDefaultAsync(ct);
        return product switch
        {
            not null => Results.Ok(product),
            null => Results.NotFound()
        };
    }

    public static async Task<IResult> Search(
        string text,
        AppDbContext dbContext,
        CancellationToken ct,
        int count = 5)
    {
        var products = await dbContext.Products
            .Where(p => EF.Functions.ILike(p.Name, $"%{text}%"))
            .Take(count)
            .ProjectToType<ProductRes>()
            .ToListAsync(ct);
        return Results.Ok(products);
    }

    public static async Task<IResult> IncrementStock(
        StockIncrementReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var errors))
        {
            return ValidationErrorRes.BadRequest(errors);
        }
        var product = await dbContext.Products.FindAsync(
            new object[] { req.Id },
            cancellationToken: ct);
        if (product is null)
        {
            return Results.NoContent();
        }
        product.AddStock(req.StockCount);
        await dbContext.SaveChangesAsync(ct);
        return Results.Ok(product.Adapt<ProductRes>());
    }
}