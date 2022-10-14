namespace SalesManagementSystem.Server.ApiContracts.Product;

using System.ComponentModel.DataAnnotations;

public sealed record CreateReq(
    [property: Required, MinLength(3)] string Name,
    [property: Required, Range(1, int.MaxValue)] int BuyingPrice,
    [property: Required, Range(1, int.MaxValue)] int SellingPrice,
    [property: Required, Range(0, int.MaxValue)] int StockCount
);

public sealed record ProductRes(
    Guid Id,
    string Name,
    int BuyingPrice,
    int SellingPrice,
    int StockCount
);

public sealed record StockIncrementReq(
    [property: Required] Guid Id,
    [property: Required, Range(1, int.MaxValue)] int StockCount
);