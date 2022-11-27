namespace SalesManagementSystem.Blazor.Pages.Product;

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
    public required ProductsClient ProductsClient { get; set; }

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
        var apiResult = SearchText switch
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
        DialogParameters parameters = new()
        {
            { "Product", product }
        };
        var dialogRef = DialogService.Show<AddStockDialog>($"Add Stock to {product.Name}", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        await LoadData();
    }

    async Task ShowDeletePromt(ProductRes product)
    {
        DialogParameters parameters = new()
        {
            { "ContentText", $"Are you sure you want to delete this product? Name: {product.Name}" },
            { "ButtonText", "Delete" },
            { "OkButtonColor", Color.Error }
        };
        var dialogRef = DialogService.Show<Prompt>("Delete Product", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        var res = await ProductsClient.Delete(product.Id);
        if (res.IsFailure)
        {
            throw new Exception(res.Error.Message);
        }
        await LoadData();
    }
}