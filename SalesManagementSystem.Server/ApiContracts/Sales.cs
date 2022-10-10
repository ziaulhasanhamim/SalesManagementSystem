namespace SalesManagementSystem.Server.ApiContracts.Sales;

using System.ComponentModel.DataAnnotations;

public sealed record CreateGroupReq(
    [property: Required, MinLength(3)] string Name);

public sealed record CreateGroupRes(Guid Id, string Name);