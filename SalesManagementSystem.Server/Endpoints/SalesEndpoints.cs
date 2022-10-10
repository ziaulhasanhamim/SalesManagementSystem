namespace SalesManagementSystem.Server.Endpoints;

using SalesManagementSystem.Server.ApiContracts.Sales;

public static class SalesEndpoints
{
    public static void Configure(WebApplication app)
    {
    }

    // public static async ValueTask<IResult> CreateGroup(
    //     CreateGroupReq req, AppDbContext dbContext, CancellationToken ct)
    // {
    //     if (!MiniValidator.TryValidate(req, out var errors))
    //     {
    //         return ValidationErrorRes.BadRequest(errors);
    //     }
    //     var salesGroup = req.Adapt<SalesGroup>();
    //     await dbContext.AddAsync(salesGroup, ct);
    //     await dbContext.SaveChangesAsync(ct);
    //     return Results.Created();
    // }
}