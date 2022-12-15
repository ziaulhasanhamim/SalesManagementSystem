namespace SalesManagementSystem.Server.Endpoints;

using System.Diagnostics.CodeAnalysis;
using PhoneNumbers;
using SalesManagementSystem.Contracts.Customer;

public static class CustomerEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/customers", Create).RequireAuthorization();
        app.MapGet("/api/customers", GetAll).RequireAuthorization();
        app.MapGet("/api/customers/{id}", Get).RequireAuthorization();
        app.MapDelete("/api/customers/{id}", Delete)
            .RequireAuthorization(options => options.RequireRole(UserRoles.Admin));
        app.MapGet("/api/customers/search-name/{name?}", SearchByName).RequireAuthorization();
        app.MapGet("/api/customers/search-number/{number?}", SearchByPhoneNumber).RequireAuthorization();
    }

    public static async ValueTask<IHttpResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        var customer = Customer.Create(req.Name, req.PhoneNumber, req.RegionCode);
        if (customer is null)
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.PhoneNumber)] = new[] { "Phone number is invalid" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        if (await customer.IsPhoneNumberDuplicate(dbContext, ct))
        {
            Dictionary<string, IEnumerable<string>> errors = new()
            {
                [nameof(CreateReq.PhoneNumber)] = new[] { "A previous customer already had this phone number" }
            };
            return HttpHelpers.BadRequest(errors);
        }
        await dbContext.AddAsync(customer, ct);
        await dbContext.SaveChangesAsync(ct);
        var uri = $"/api/customer/{customer.Id}";
        return HttpResults.Created(uri, new CustomerRes(
            customer.Id,
            customer.Name,
            customer.PhoneNumber,
            0
        ));
    }

    public static async Task<IHttpResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var customers = await dbContext.Customers
            .Select(c => new CustomerRes(
                c.Id,
                c.Name,
                c.PhoneNumber,
                c.Purchases!.Sum(s => s.SoldPrice * s.Quantity)
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(customers);
    }

    public static async Task<IHttpResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var customer = await dbContext.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerRes(
                c.Id,
                c.Name,
                c.PhoneNumber,
                c.Purchases!.Sum(s => s.SoldPrice * s.Quantity)
            ))
            .FirstOrDefaultAsync(ct);
        return customer switch
        {
            not null => HttpResults.Ok(customer),
            null => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> Delete(
        Guid id,
        AppDbContext dbContext,
        CancellationToken ct)
    {
        var deleted = await dbContext.Customers
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);
        return deleted switch
        {
            1 => HttpResults.NoContent(),
            _ => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> SearchByPhoneNumber(
        string? number,
        AppDbContext dbContext,
        CancellationToken ct,
        int? count)
    {
        var customers = await dbContext.Customers
            .WhereIf(!string.IsNullOrEmpty(number), c => EF.Functions.Like(c.PhoneNumber, $"%{number}%"))
            .TakeIfNotNull(count < 1 ? 20 : count)
            .Select(c => new CustomerRes(
                c.Id,
                c.Name,
                c.PhoneNumber,
                c.Purchases!.Sum(s => s.SoldPrice * s.Quantity)
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(customers);
    }

    public static async Task<IHttpResult> SearchByName(
        string? name,
        AppDbContext dbContext,
        CancellationToken ct,
        int? count)
    {
        var takenCustomers = await dbContext.Customers
            .WhereIf(!string.IsNullOrEmpty(name), c => EF.Functions.ILike(c.Name, $"%{name}%"))
            .TakeIfNotNull(count < 1 ? 20 : count)
            .Select(c => new CustomerRes(
                c.Id,
                c.Name,
                c.PhoneNumber,
                c.Purchases!.Sum(s => s.SoldPrice * s.Quantity)
            ))
            .ToListAsync(ct);
        return HttpResults.Ok(takenCustomers);
    }
}