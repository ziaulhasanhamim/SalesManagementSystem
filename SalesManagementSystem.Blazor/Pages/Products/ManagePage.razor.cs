namespace SalesManagementSystem.Blazor.Pages.Products;

using SalesManagementSystem.Contracts.Product;

public sealed partial class ManagePage
{
    IReadOnlyList<ProductRes> _products = Array.Empty<ProductRes>();
    bool _loading = true;
    bool _shouldReload = true;
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
    public ProductsClient ProductsClient { get; set; } = null!;

    [Inject]
    public IDialogService DialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await base.OnInitializedAsync();
    }

    async Task LoadData()
    {
        _loading = true;
        StateHasChanged();
        var apiResult = _searchText switch
        {
            "" or null => await ProductsClient.GetAll(),
            _ => await ProductsClient.Search(SearchText)
        };
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _products = apiResult.Value;
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

    async Task ShowStockIncementDialog(ProductRes product)
    {
        DialogParameters parameters = new();
        parameters.Add("Product", product);
        var dialogRef = DialogService.Show<AddStockDialog>($"Add Stock to {product.Name}", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        await LoadData();
    }
}