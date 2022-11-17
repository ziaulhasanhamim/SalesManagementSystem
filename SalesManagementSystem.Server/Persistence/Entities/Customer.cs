namespace SalesManagementSystem.Server.Persistence.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoneNumbers;

public sealed class Customer
{
    private Customer() {}

    [Key]
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string PhoneNumber { get; set; }

    [InverseProperty(nameof(SalesEntry.Customer))]
    public ICollection<SalesEntry>? Purchases { get; set; }

    public static Customer? Create(string name, string number, string? regionCode)
    {
        var util = PhoneNumberUtil.GetInstance();
        if (util.TryParse(number, regionCode, out var phoneNumber))
        {
            var phoneNumberStr = $"+{phoneNumber.CountryCode}{phoneNumber.NationalNumber}";
            return new Customer()
            {
                Name = name,
                PhoneNumber = phoneNumberStr
            };
        }
        return null;
    }

    public Task<bool> IsPhoneNumberDuplicate(AppDbContext dbContext, CancellationToken ct = default) =>
        dbContext.Customers.AnyAsync(c => c.PhoneNumber == PhoneNumber, ct);
}