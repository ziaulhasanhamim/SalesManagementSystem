namespace SalesManagementSystem.Server.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly EFCoreFastAuthOptions<User> _authOptions;
    public AppDbContext(DbContextOptions<AppDbContext> options, EFCoreFastAuthOptions<User> authOptions)
        : base(options)
    {
        _authOptions = authOptions;
    }

    public DbSet<SalesEntry> SalesEntries { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("pg_trgm");

        builder.ConfigureAuthModels(_authOptions);

        builder.Entity<Product>()
            .HasIndex(p => p.Name)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.Entity<Customer>()
            .HasIndex(c => c.Name)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber, "IX_Customers_PhoneNumber");

        builder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber, "IX_Customers_PhoneNumber_GIN")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.Entity<SalesEntry>()
            .HasIndex(s => s.TransactionTime);
    }

    public void Detach<T>(T entity)
        where T : notnull
    {
        Entry(entity).State = EntityState.Detached;
    }
}