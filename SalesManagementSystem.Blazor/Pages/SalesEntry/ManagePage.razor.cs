namespace SalesManagementSystem.Blazor.Pages.SalesEntry;

using SalesManagementSystem.Contracts.SalesEntry;

public sealed partial class ManagePage
{
    SalesDataRes? _salesData;
    bool _loading = true;

    SalesTimeRange _salesTimeRange = SalesTimeRange.Today;

    IReadOnlyList<SalesEntryRes> _salesEntries = Array.Empty<SalesEntryRes>();

    [Inject]
    public required SalesEntriesClient SalesEntriesClient { get; init; }

    [Inject]
    public required IDialogService DialogService { get; init; }

    [Inject]
    public required IAuthService AuthService { get; init; }

    SalesTimeRange SalesTimeRange
    {
        get => _salesTimeRange;
        set
        {
            _salesTimeRange = value;
            SalesTimeRangeChanged();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await base.OnInitializedAsync();
    }

    async void SalesTimeRangeChanged()
    {
        await LoadData();
        StateHasChanged();
    }

    async Task LoadData()
    {
        _loading = true;
        StateHasChanged();
        var salesResult = await SalesEntriesClient.GetSales(SalesTimeRange);
        if (salesResult.IsFailure)
        {
            throw new Exception(salesResult.Error.Message);
        }
        _salesEntries = salesResult.Value;
        var salesDataResult = await SalesEntriesClient.GetSalesData(SalesTimeRange);
        if (salesDataResult.IsFailure)
        {
            throw new Exception(salesDataResult.Error.Message);
        }
        _salesData = salesDataResult.Value;
        _loading = false;
    }

    async Task ShowDeletePromt(SalesEntryRes salesEntry)
    {
        DialogParameters parameters = new()
        {
            { "ContentText", "Are you sure you want to delete this?" },
            { "ButtonText", "Delete" },
            { "OkButtonColor", Color.Error }
        };
        var dialogRef = DialogService.Show<Prompt>("Delete Sales Entry", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        var res = await SalesEntriesClient.Delete(salesEntry.Id);
        if (res.IsFailure)
        {
            throw new Exception(res.Error.Message);
        }
        await LoadData();
    }
}