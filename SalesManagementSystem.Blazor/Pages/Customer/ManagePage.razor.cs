namespace SalesManagementSystem.Blazor.Pages.Customer;

using SalesManagementSystem.Contracts.Customer;

public sealed partial class ManagePage
{
    IReadOnlyList<CustomerRes> _customers = Array.Empty<CustomerRes>();
    bool _loading = true;
    bool _shouldReload = true;
    bool _searchByNumber;
    string _searchText = "";

    private string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            RefreshData();
        }
    }

    [Inject]
    public required CustomersClient CustomersClient { get; set; }

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
        var apiResult = (_searchText, _searchByNumber) switch
        {
            ("", _) or (null, _) => await CustomersClient.GetAll(),
            (_, false) => await CustomersClient.SearchByName(SearchText),
            (_, true) => await CustomersClient.SearchByPhoneNumber(SearchText),
        };
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _customers = apiResult.Value;
        _loading = false;
    }

    private async void RefreshData()
    {
        if (!_shouldReload)
        {
            return;
        }
        _shouldReload = true;
        await Task.Delay(500);
        await LoadData();
        StateHasChanged();
    }

    async Task ShowDeletePromt(CustomerRes customer)
    {
        DialogParameters parameters = new()
        {
            { "ContentText", "Are you sure you want to delete this customer?" },
            { "ButtonText", "Delete" },
            { "OkButtonColor", Color.Error }
        };
        var dialogRef = DialogService.Show<Prompt>("Delete customer", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        var res = await CustomersClient.Delete(customer.Id);
        if (res.IsFailure)
        {
            throw new Exception(res.Error.Message);
        }
        await LoadData();
    }
}