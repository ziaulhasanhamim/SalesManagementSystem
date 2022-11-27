namespace SalesManagementSystem.Blazor.Pages.PaymentMethod;

using SalesManagementSystem.Contracts.PaymentMethod;

public sealed partial class ManagePage
{
    IReadOnlyList<PaymentMethodRes> _payments = Array.Empty<PaymentMethodRes>();
    bool _loading = true;

    [Inject]
    public required PaymentMethodsClient PaymentsClient { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

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

    async Task ShowDeletePromt(PaymentMethodRes paymentMethod)
    {
        DialogParameters parameters = new()
        {
            { "ContentText", "Are you sure you want to delete this?" },
            { "ButtonText", "Delete" },
            { "OkButtonColor", Color.Error }
        };
        var dialogRef = DialogService.Show<Prompt>("Delete Item", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        var res = await PaymentsClient.Delete(paymentMethod.Id);
        if (res.IsFailure)
        {
            throw new Exception(res.Error.Message);
        }
        await LoadData();
    }
}