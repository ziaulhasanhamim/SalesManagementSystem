namespace SalesManagementSystem.Server.Persistence.Entities;

public sealed class User : FastUser
{
    public List<string>? Roles { get; set; }
}