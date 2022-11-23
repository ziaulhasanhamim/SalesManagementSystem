namespace SalesManagementSystem.Contracts;

public sealed record UnauthorizedError(
    string Message = "User is not authorized to access this page"
)
    : Error(Message, nameof(UnauthorizedError));