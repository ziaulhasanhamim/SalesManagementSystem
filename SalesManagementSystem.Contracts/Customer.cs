namespace SalesManagementSystem.Contracts.Customers;

using System.ComponentModel.DataAnnotations;

public sealed record CreateReq(
    [property: Required, MinLength(3)] string Name,
    [property: Required, MinLength(11)] string PhoneNumber,
    string? RegionCode
);

public sealed record CustomerRes(
    Guid Id,
    string Name,
    string PhoneNumber
);