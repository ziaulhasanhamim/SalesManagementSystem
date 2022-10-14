namespace SalesManagementSystem.Server.ApiContracts.SalesEntry;

using System.ComponentModel.DataAnnotations;
using ProductContracts = SalesManagementSystem.Server.ApiContracts.Product.ProductRes;

public sealed record CreateReq(
    [property: Required] Guid ProductId,
    [property: Required, Range(1, int.MaxValue)] int Quantity,
    [property: Required, Range(1, int.MaxValue)] int SoldPrice,
    [property: Required] Guid PaymentMethodId,
    Guid? CustomerId
);

public sealed record SalesEntryRes(
    Guid Id,
    Guid ProductId,
    int Quantity,
    int SoldPrice,
    string PaymentMethod,
    DateTime TransactionTime,
    Guid? CustomerId
);
