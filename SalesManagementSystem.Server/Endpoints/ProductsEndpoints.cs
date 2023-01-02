namespace SalesManagementSystem.Server.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.Contracts.Product;

public static class ProductEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/products", Create)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
        app.MapGet("/api/products", GetAll).RequireAuthorization();
        app.MapGet("/api/products/{id}", Get).RequireAuthorization();
        app.MapDelete("/api/products/{id}", Delete)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
        app.MapGet("/api/products/search/{text?}", Search).RequireAuthorization();
        app.MapGet("/api/products/search-sellables/{text?}", SearchSellables).RequireAuthorization();
        app.MapPost("/api/products/incement-stock", IncrementStock).RequireAuthorization();
        app.MapPost("/api/products/change-deprecation-state", ChangeDeprecationState)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
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
        await dbContext.AddAsync(product, ct);
        await dbContext.SaveChangesAsync(ct);
        var res = new ProductRes(
            product.Id,
            product.Name,
            product.BuyingPrice,
            product.SellingPrice,
            product.StockCount,
            product.IsDeprecated
        );
        var uri = $"/api/products/{product.Id}";
        return HttpResults.Created(uri, res);
    }

    public static async Task<IHttpResult> GetAll(
        HttpContext ctx,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var isAdmin = ctx.User.HasClaim(ClaimTypes.Role, UserRoles.Admin);
        var products = await dbContext.Products
            .Select(p => new ProductRes(
                p.Id,
                p.Name,
                isAdmin ? p.BuyingPrice : default,
                p.SellingPrice,
                p.StockCount,
                p.IsDeprecated
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(products);
    }

    public static async Task<IHttpResult> Get(
        Guid id,
        HttpContext ctx,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var isAdmin = ctx.User.HasClaim(ClaimTypes.Role, UserRoles.Admin);
        var product = await dbContext.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductRes(
                p.Id,
                p.Name,
                isAdmin ? p.BuyingPrice : default,
                p.SellingPrice,
                p.StockCount,
                p.IsDeprecated
            ))
            .FirstOrDefaultAsync(ct);
        return product switch
        {
            not null => HttpResults.Ok(product),
            null => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> Delete(
        Guid id,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var deleted = await dbContext.Products
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);
        return deleted switch
        {
            1 => HttpResults.NoContent(),
            _ => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> Search(
        string? text,
        HttpContext ctx,
        AppDbContext dbContext,
        CancellationToken ct,
        int? count)
    {
        var isAdmin = ctx.User.HasClaim(ClaimTypes.Role, UserRoles.Admin);
        var products = await dbContext.Products
            .WhereIf(
                !string.IsNullOrEmpty(text),
                p => EF.Functions.ILike(p.Name, $"%{text}%"))
            .TakeIfNotNull(count < 1 ? 20 : count)
            .Select(p => new ProductRes(
                p.Id,
                p.Name,
                isAdmin ? p.BuyingPrice : default,
                p.SellingPrice,
                p.StockCount,
                p.IsDeprecated
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(products);
    }

    public static async Task<IHttpResult> SearchSellables(
        string? text,
        HttpContext ctx,
        AppDbContext dbContext,
        CancellationToken ct,
        int? count)
    {
        var isAdmin = ctx.User.HasClaim(ClaimTypes.Role, UserRoles.Admin);
        var products = await dbContext.Products
            .Where(p => !p.IsDeprecated)
            .WhereIf(
                !string.IsNullOrEmpty(text),
                p => EF.Functions.ILike(p.Name, $"%{text}%"))
            .TakeIfNotNull(count < 1 ? 20 : count)
            .Select(p => new ProductRes(
                p.Id,
                p.Name,
                isAdmin ? p.BuyingPrice : default,
                p.SellingPrice,
                p.StockCount,
                p.IsDeprecated
            ))
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
        var updateCount = await dbContext.Products
            .Where(p => p.Id == req.Id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    p => p.StockCount,
                    p => p.StockCount + req.StockCount),
                ct);
        if (updateCount is not 1)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(req.Id)] = new[] { "Product id not found" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        return HttpResults.Ok();
    }

    public static async Task<IHttpResult> ChangeDeprecationState(
        ChangeDeprecationStateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var updateCount = await dbContext.Products
            .Where(p => p.Id == req.Id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    p => p.IsDeprecated,
                    _ => req.IsDeprecated),
                ct);
        if (updateCount is not 1)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(req.Id)] = new[] { "Product id not found" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        return HttpResults.Ok();
    }
}