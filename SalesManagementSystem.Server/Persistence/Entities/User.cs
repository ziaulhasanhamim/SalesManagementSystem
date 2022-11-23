namespace SalesManagementSystem.Server.Persistence.Entities;

public sealed class User : FastUser
{
    public string? Role { get; set; }
}