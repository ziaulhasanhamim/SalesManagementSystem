namespace SalesManagementSystem.Contracts.PaymentMethods;

using System.ComponentModel.DataAnnotations;

public sealed record CreateReq(
    [property: Required, MinLength(3)] string Name
);

public sealed record PaymentMethodRes(
    Guid Id,
    string Name
);