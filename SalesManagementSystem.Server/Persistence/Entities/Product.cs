namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;

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

    [Required]
    public bool IsDeprecated { get; set; }

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