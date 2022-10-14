namespace SalesManagementSystem.Server.ApiContracts;

public sealed record ValidationErrorRes(
    IDictionary<string, IEnumerable<string>> Errors,
    string Message = "Provided request data was not valid")
{
    public static IResult BadRequest(
        IDictionary<string, IEnumerable<string>> errors,
        string message = "Provided request data was not valid"
    ) =>
        Results.BadRequest(new ValidationErrorRes(errors, message));

    public static IResult BadRequest(
        IDictionary<string, string[]> errors,
        string message = "Provided request data was not valid")
    {
        Dictionary<string, IEnumerable<string>> newErrors = new(errors.Count);
        foreach (var errKeyValue in errors)
        {
            newErrors[errKeyValue.Key] = errKeyValue.Value;
        }
        return BadRequest(newErrors, message);
    }

    public static IResult BadRequest(ValidationError validationError)
    {
        Dictionary<string, IEnumerable<string>> errors = new();
        foreach (var error in validationError.ErrorInfos)
        {
            if (errors.TryGetValue(error.Key, out var errList))
            {
                ((List<string>)errList).Add(error.Message);
                continue;
            }
            errors[error.Key] = new List<string>() { error.Message };
        }
        return BadRequest(errors, validationError.Message);
    }
}