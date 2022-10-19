namespace SalesManagementSystem.Server.Helpers;

public static class HttpHelpers
{
    public static IHttpResult BadRequest(
        IDictionary<string, IEnumerable<string>> errors,
        string message = "Provided request data was not valid"
    ) =>
        HttpResults.BadRequest(new ValidationErrorRes(errors, message));

    public static IHttpResult BadRequest(
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

    public static IHttpResult BadRequest(ValidationError validationError)
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