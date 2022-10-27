namespace SalesManagementSystem.Blazor.Pages.Product;

using SalesManagementSystem.Contracts.Product;

public sealed partial class AddPage
{
    readonly InputModel _input = new();
    EditContext _formCtx = null!;
    ValidationMessageStore _messageStore = null!;
    bool _loading;

    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [Inject]
    public ProductsClient ProductsClient { get; init; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; init; } = null!;

    protected override void OnInitialized()
    {
        _formCtx = new(_input);
        _messageStore = new(_formCtx);
        _formCtx.OnFieldChanged += (_, ae) =>
        {
            _messageStore.Clear(ae.FieldIdentifier);
            _formCtx.NotifyValidationStateChanged();
        };
        base.OnInitialized();
    }

    async Task OnValidSubmit()
    {
        _loading = true;
        var result = await ProductsClient.Create(new CreateReq(
            _input.Name,
            _input.BuyingPrice,
            _input.SellingPrice,
            _input.StockCount
        ), default);

        result.Switch(
            _ => NavigationManager.NavigateTo(ReturnUrl ?? "/products/"),
            err =>
            {
                if (err is ValidationErrorRes { Errors: var errsDict })
                {
                    AddErrsToStore(errsDict);
                    _formCtx.NotifyValidationStateChanged();
                    return;
                }
                throw new Exception(err.Message);
            }
        );
        _loading = false;
    }

    void AddErrsToStore(IDictionary<string, IEnumerable<string>> errsDict)
    {
        foreach (var errsKeyVal in errsDict)
        {
            _messageStore.Add(
                new FieldIdentifier(_input, errsKeyVal.Key),
                errsKeyVal.Value);
        }
    }

    public sealed class InputModel
    {
        [Required, MinLength(3)]
        public string Name { get; set; } = "";

        [Required, Range(1, int.MaxValue)]
        public int BuyingPrice { get; set; } = 1;

        [Required, Range(2, int.MaxValue)]
        public int SellingPrice { get; set; } = 2;

        [Required, Range(0, int.MaxValue)]
        public int StockCount { get; set; }
    }
}