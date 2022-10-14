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
    public int BuyingPrice { get; set; }

    [Required]
    public int SellingPrice { get; set; }

    [Required]
    public int StockCount { get; private set; }

    public Task<bool> IsNameDuplicate(AppDbContext dbContext, CancellationToken ct = default) =>
        dbContext.Products.AnyAsync(p => EF.Functions.ILike(p.Name, Name), ct);

    public void AddStock(int count) => StockCount += count;

    public bool TryRemoveStock(int count)
    {
        if (StockCount < count)
        {
            return false;
        }
        StockCount -= count;
        return true;
    }
}