namespace SalesManagementSystem.Blazor.Pages.SalesEntry;

using SalesManagementSystem.Contracts.SalesEntry;

public sealed partial class ManagePage
{
    IReadOnlyList<SalesEntryRes> _salesEntries = Array.Empty<SalesEntryRes>();
    bool _loading = true;

    [Inject]
    public SalesEntriesClient SalesEntriesClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await base.OnInitializedAsync();
    }

    async Task LoadData()
    {
        _loading = true;
        StateHasChanged();
        var apiResult = await SalesEntriesClient.GetAll();
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _salesEntries = apiResult.Value;
        _loading = false;
    }
}