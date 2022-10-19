namespace SalesManagementSystem.Contracts;

using System.Text.Json.Serialization;

public sealed record NotFoundRes(string Message) : IError
{
    [JsonIgnore]
    public string? Code => nameof(NotFoundError);
}