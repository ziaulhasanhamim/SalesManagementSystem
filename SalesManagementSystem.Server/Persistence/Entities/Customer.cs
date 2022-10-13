namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoneNumbers;

public sealed class Customer
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string PhoneNumber { get; set; } = "";

    [InverseProperty(nameof(SalesEntry.Customer))]
    public ICollection<SalesEntry>? Purchases { get; set; }

    public bool TrySetPhoneNumber(string number, string? regionCode)
    {
        var util = PhoneNumberUtil.GetInstance();
        if (util.TryParse(number, regionCode, out var phoneNumber))
        {
            PhoneNumber = util.Format(phoneNumber, PhoneNumberFormat.INTERNATIONAL);
            return true;
        }
        return false;
    }

    public Task<bool> IsPhoneNumberDuplicate(AppDbContext dbContext, CancellationToken ct = default) =>
        dbContext.Customers.AnyAsync(c => c.PhoneNumber == PhoneNumber, ct);
}