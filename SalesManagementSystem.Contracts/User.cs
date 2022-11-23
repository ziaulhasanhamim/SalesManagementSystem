namespace SalesManagementSystem.Contracts.User;

using System.ComponentModel.DataAnnotations;

public sealed record LoginReq(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password
);

public sealed record LoginRes(
    string Token,
    DateTimeOffset ExpiresAt
);

public sealed record CreateReq(
    [property: Required, EmailAddress] string Email,

    [property: Required, MinLength(8, ErrorMessage ="Password must be at least 8 characters")]
    string Password,

    [property: Required] string Role
);

public sealed record UserRes(
    Guid Id,
    string Email,
    string Role
);