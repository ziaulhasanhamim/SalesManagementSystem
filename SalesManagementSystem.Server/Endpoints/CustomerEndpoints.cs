namespace SalesManagementSystem.Server.Endpoints;

using System.Diagnostics.CodeAnalysis;
using PhoneNumbers;
using SalesManagementSystem.Contracts.Customers;

public static class CustomerEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/customers", Create);
        app.MapGet("/api/customers", GetAll);
        app.MapGet("/api/customers/{id}", Get);
        app.MapGet("/api/customers/search-name/{name}", SearchByName);
        app.MapGet("/api/customers/search-number/{number}", SearchByPhoneNumber);
    }

    public static async ValueTask<IHttpResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return HttpHelpers.BadRequest(vErrors);
        }
        Customer customer = new()
        {
            Name = req.Name,
        };
        if (!customer.TrySetPhoneNumber(req.PhoneNumber, req.RegionCode))
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
        return HttpResults.Created(uri, customer.Adapt<CustomerRes>());
    }

    public static async Task<IHttpResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var customers = await dbContext.Customers
            .ProjectToType<Customer>()
            .ToListAsync(ct);
        return HttpResults.Ok(customers);
    }

    public static async Task<IHttpResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var customer = await dbContext.Customers
            .Where(c => c.Id == id)
            .ProjectToType<CustomerRes>()
            .FirstOrDefaultAsync(ct);
        return customer switch
        {
            not null => HttpResults.Ok(customer),
            null => HttpResults.NotFound()
        };
    }

    public static async Task<IHttpResult> SearchByPhoneNumber(
        string number,
        AppDbContext dbContext,
        CancellationToken ct,
        int? count = null)
    {
        Range range = count switch
        {
            null => ..,
            > 0 and int val => ..val,
            _ => ..100
        };
        var customers = await dbContext.Customers
            .Where(c => EF.Functions.Like(c.PhoneNumber, $"%{number}%"))
            .Take(range)
            .ProjectToType<CustomerRes>()
            .ToListAsync(ct);
        return HttpResults.Ok(customers);
    }

    public static async Task<IHttpResult> SearchByName(
        string name,
        AppDbContext dbContext,
        CancellationToken ct,
        int? count = null)
    {
        Range range = count switch
        {
            null => ..,
            > 0 and int val => ..val,
            _ => ..100
        };
        var customers = await dbContext.Customers
            .Where(c => EF.Functions.ILike(c.Name, $"%{name}%"))
            .Take(range)
            .ProjectToType<CustomerRes>()
            .ToListAsync(ct);
        return HttpResults.Ok(customers);
    }
}