namespace SalesManagementSystem.Server.ApiContracts;

public sealed record ValidationErrorRes(
    IDictionary<string, string[]> Errors,
    string Message = "Provided request data was not valid")
{
    public static IResult BadRequest(
        IDictionary<string, string[]> errors,
        string message = "Provided request data was not valid"
    ) =>
        Results.BadRequest(new ValidationErrorRes(errors, message));
}