namespace SalesManagementSystem.Blazor.Pages.SalesEntry;

using SalesManagementSystem.Contracts.SalesEntry;

public sealed partial class ManagePage
{
    SalesDataRes? _salesData;
    bool _loading = true;

    SalesOfType _salesOf = SalesOfType.Today;

    IReadOnlyList<SalesEntryRes> _salesEntries = Array.Empty<SalesEntryRes>();

    [Inject]
    public SalesEntriesClient SalesEntriesClient { get; set; } = null!;

    SalesOfType SalesOf
    {
        get => _salesOf;
        set
        {
            _salesOf = value;
            SalesOfChanged();
        }
    }

    uint? Days => SalesOf switch
    {
        SalesOfType.Today => 1,
        SalesOfType.ThisWeek => 7,
        SalesOfType.ThisMonth => 30,
        SalesOfType.ThisYear => 365,
        _ => null,
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await base.OnInitializedAsync();
    }

    async void SalesOfChanged()
    {
        await LoadData();
        StateHasChanged();
    }

    async Task LoadData()
    {
        _loading = true;
        StateHasChanged();
        var salesDataResult = await SalesEntriesClient.GetSalesData(Days);
        if (salesDataResult.IsFailure)
        {
            throw new Exception(salesDataResult.Error.Message);
        }
        _salesData = salesDataResult.Value;
        var salesResult = await SalesEntriesClient.GetSales(Days);
        if (salesResult.IsFailure)
        {
            throw new Exception(salesResult.Error.Message);
        }
        _salesEntries = salesResult.Value;
        _loading = false;
    }

    enum SalesOfType
    {
        Today, ThisWeek, ThisMonth, ThisYear, All
    }
}