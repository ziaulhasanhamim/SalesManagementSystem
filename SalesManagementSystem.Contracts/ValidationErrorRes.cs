namespace SalesManagementSystem.Contracts;

using System.Text.Json.Serialization;

public sealed record ValidationErrorRes(
    IDictionary<string, IEnumerable<string>> Errors,
    string Message = "Provided request data was not valid")
    : Error(Message)
{
    [JsonIgnore]
    public new string? Code => nameof(ValidationErrorRes);
}