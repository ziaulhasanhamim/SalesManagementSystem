namespace SalesManagementSystem.Blazor.Pages.PaymentMethod;

using SalesManagementSystem.Contracts.PaymentMethod;

public sealed partial class ManagePage
{
    IReadOnlyList<PaymentMethodRes> _payments = Array.Empty<PaymentMethodRes>();
    bool _loading = true;

    [Inject]
    public PaymentMethodsClient PaymentsClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await base.OnInitializedAsync();
    }

    async Task LoadData()
    {
        _loading = true;
        StateHasChanged();
        var apiResult = await PaymentsClient.GetAll();
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _payments = apiResult.Value;
        _loading = false;
    }
}