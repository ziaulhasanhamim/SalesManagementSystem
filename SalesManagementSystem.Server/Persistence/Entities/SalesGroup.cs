namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class SalesGroup
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [InverseProperty(nameof(SalesEntry.SalesGroup))]
    public List<SalesEntry>? SalesEntries { get; set; }
}