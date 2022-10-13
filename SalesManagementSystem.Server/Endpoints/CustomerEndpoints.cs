namespace SalesManagementSystem.Server.Endpoints;

using System.Diagnostics.CodeAnalysis;
using PhoneNumbers;
using SalesManagementSystem.Server.ApiContracts.Customers;

public static class CustomerEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/customers", Create);
        app.MapGet("/api/customers", GetAll);
        app.MapGet("/api/customers/{id}", Get);
        app.MapGet("/api/customers/search-name/{nameText}", SearchByName);
        app.MapGet("/api/customers/search-number/{number}", SearchByPhoneNumber);
    }

    public static async ValueTask<IResult> Create(
        CreateReq req, AppDbContext dbContext, CancellationToken ct)
    {
        if (!MiniValidator.TryValidate(req, out var vErrors))
        {
            return ValidationErrorRes.BadRequest(vErrors);
        }
        Customer customer = new()
        {
            Name = req.Name,
        };
        if (!customer.TrySetPhoneNumber(req.PhoneNumber, req.RegionCode))
        {
            Dictionary<string, string[]> errors = new()
            {
                [nameof(CreateReq.PhoneNumber)] = new[] { "Phone number is invalid" }
            };
            return ValidationErrorRes.BadRequest(errors);
        }
        if (await customer.IsPhoneNumberDuplicate(dbContext, ct))
        {
            Dictionary<string, string[]> errors = new()
            {
                [nameof(CreateReq.PhoneNumber)] = new[] { "A previous customer already had this phone number" }
            };
            return ValidationErrorRes.BadRequest(errors);
        }
        await dbContext.AddAsync(customer, ct);
        await dbContext.SaveChangesAsync(ct);
        var uri = $"/api/customer/{customer.Id}";
        return Results.Created(uri, customer.Adapt<CustomerRes>());
    }

    public static async Task<IResult> GetAll(AppDbContext dbContext, CancellationToken ct)
    {
        var customers = await dbContext.Customers
            .ProjectToType<Customer>()
            .ToListAsync(ct);
        return Results.Ok(customers);
    }

    public static async Task<IResult> Get(Guid id, AppDbContext dbContext, CancellationToken ct)
    {
        var customer = await dbContext.Customers
            .ProjectToType<CustomerRes>()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        return customer switch
        {
            not null => Results.Ok(customer),
            null => Results.NotFound()
        };
    }

    public static async Task<IResult> SearchByPhoneNumber(
        string number,
        AppDbContext dbContext,
        CancellationToken ct,
        int count = 5)
    {
        var customers = await dbContext.Customers
            .Where(c => EF.Functions.Like(c.PhoneNumber, $"%{number}%"))
            .Take(count)
            .ProjectToType<CustomerRes>()
            .ToListAsync(ct);
        return Results.Ok(customers);
    }

    public static async Task<IResult> SearchByName(
        string nameText,
        AppDbContext dbContext,
        CancellationToken ct,
        int count = 5)
    {
        var customers = await dbContext.Customers
            .Where(c => EF.Functions.ILike(c.Name, $"%{nameText}%"))
            .Take(count)
            .ProjectToType<CustomerRes>()
            .ToListAsync(ct);
        return Results.Ok(customers);
    }
}