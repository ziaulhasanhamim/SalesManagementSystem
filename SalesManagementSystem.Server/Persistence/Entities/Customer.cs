namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class Customer
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string PhoneNumer { get; set; } = "";

    [InverseProperty(nameof(SalesEntry.Customer))]
    public ICollection<SalesEntry>? Purchases { get; set; }
}