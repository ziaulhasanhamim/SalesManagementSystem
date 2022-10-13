namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;

[Index(nameof(Name))]
public sealed class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public int Price { get; set; }

    [Required]
    public int StockCount { get; set; }

    public Task<bool> IsNameDuplicate(AppDbContext dbContext, CancellationToken ct = default) =>
        dbContext.Products.AnyAsync(p => EF.Functions.ILike(p.Name, Name), ct);
}