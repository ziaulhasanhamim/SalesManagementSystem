namespace SalesManagementSystem.Contracts.SalesEntry;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public sealed record CreateReq(
    [property: Required] Guid ProductId,
    [property: Required, Range(1, int.MaxValue)] int Quantity,
    [property: Required, Range(1, int.MaxValue)] int SoldPrice,
    [property: Required] Guid PaymentMethodId,
    Guid? CustomerId
);

public sealed record SalesEntryRes(
    Guid Id,
    string ProductName,
    int Quantity,
    int SoldPrice,
    int BuyingPrice,
    string PaymentMethod,
    DateTime TransactionTime,
    string? CustomerPhoneNumber,
    string? CustomerName)
{
    [JsonIgnore]
    public int Profit => SoldPrice - BuyingPrice;
}