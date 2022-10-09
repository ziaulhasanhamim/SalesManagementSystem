namespace SalesManagementSystem.Server.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SalesEntry> SalesEntries { get; set; } = null!;

    public DbSet<SalesGroup> SalesGroups { get; set; } = null!;
}