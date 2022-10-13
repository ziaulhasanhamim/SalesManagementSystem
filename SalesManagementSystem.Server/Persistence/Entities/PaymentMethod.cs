namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class PaymentMethod
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    public Task<bool> IsNameDuplicate(AppDbContext dbContext, CancellationToken ct = default) =>
        dbContext.PaymentMethods.AnyAsync(p => EF.Functions.ILike(p.Name, Name), ct);
}