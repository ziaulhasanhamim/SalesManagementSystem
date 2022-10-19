namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Contracts.Product;

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

    public static async ValueTask<IHttpResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        if (req.SellingPrice <= req.BuyingPrice)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.SellingPrice)] = new[] { "Selling price must be greater than buying price" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        var product = req.Adapt<Product>();
        if (await product.IsNameDuplicate(dbContext, ct))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.Name)] = new[] { "A product with same name exists" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        await dbContext.AddAsync(product, ct);
        await dbContext.SaveChangesAsync(ct);
        var res = product.Adapt<ProductRes>();
        var uri = $"/api/products/{product.Id}";
        return HttpResults.Created(uri, res);
    }

    public static async Task<IHttpResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var products = await dbContext.Products
            .ProjectToType<ProductRes>()
            .ToListAsync(ct);
        return HttpResults.Ok(products);
    }

    public static async Task<IHttpResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var product = await dbContext.Products
            .Where(p => p.Id == id)
            .ProjectToType<ProductRes>()
            .FirstOrDefaultAsync(ct);
        return product switch
        {
            not null => HttpResults.Ok(product),
            null => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> Search(
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
        return HttpResults.Ok(products);
    }

    public static async Task<IHttpResult> IncrementStock(
        StockIncrementReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var product = await dbContext.Products.FindAsync(
            new object[] { req.Id },
            cancellationToken: ct);
        if (product is null)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(req.Id)] = new[] { "Product id not found" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        product.AddStock(req.StockCount);
        await dbContext.SaveChangesAsync(ct);
        return HttpResults.Ok(product.Adapt<ProductRes>());
    }
}