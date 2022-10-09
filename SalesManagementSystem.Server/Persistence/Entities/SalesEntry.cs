namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class SalesEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string ProductName { get; set; } = "";

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int Price { get; set; }

    [Required]
    public DateTime TransactionTime { get; set; }

    [Required]
    public Guid SalesGroupId { get; set; }

    [ForeignKey(nameof(SalesGroupId))]
    public SalesGroup? SalesGroup { get; set; }
}