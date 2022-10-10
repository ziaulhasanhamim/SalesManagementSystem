namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class SalesEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int SoldPrice { get; set; }

    [Required]
    public DateTime TransactionTime { get; set; }

    [Required]
    public Guid PaymentMethodId { get; set; }

    [ForeignKey(nameof(PaymentMethodId))]
    public PaymentMethod? PaymentMethod { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [Required]
    public Guid? CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }
}