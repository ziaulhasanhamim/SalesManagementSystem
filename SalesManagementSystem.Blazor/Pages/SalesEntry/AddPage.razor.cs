namespace SalesManagementSystem.Blazor.Pages.SalesEntry;

using SalesManagementSystem.Contracts.SalesEntry;
using ProductContracts = SalesManagementSystem.Contracts.Product;
using PaymentContracts = SalesManagementSystem.Contracts.PaymentMethod;
using CustomerContracts = SalesManagementSystem.Contracts.Customer;

public sealed partial class AddPage
{
    readonly InputModel _input = new();
    EditContext _formCtx = null!;
    ValidationMessageStore _messageStore = null!;
    bool _loading;
    private IReadOnlyList<PaymentContracts.PaymentMethodRes> _paymentMethods =
        Array.Empty<PaymentContracts.PaymentMethodRes>();

    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [Inject]
    public SalesEntriesClient SalesClient { get; init; } = null!;

    [Inject]
    public ProductsClient ProductsClient { get; init; } = null!;

    [Inject]
    public CustomersClient CustomersClient { get; init; } = null!;

    [Inject]
    public PaymentMethodsClient PaymentsClient { get; init; } = null!;

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

    protected override async Task OnInitializedAsync()
    {
        var apiResult = await PaymentsClient.GetAll();
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _paymentMethods = apiResult.Value;
        await base.OnInitializedAsync();
    }

    string ProductToString(ProductContracts.ProductRes product) => $"{product.Name}(Selling price - {product.SellingPrice}, Stock - {product.StockCount})";

    async Task<IEnumerable<ProductContracts.ProductRes>> SearchProducts(string text, CancellationToken ct)
    {
        var apiResult = await ProductsClient.Search(text, 5, ct);
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        return apiResult.Value;
    }

    async Task<IEnumerable<CustomerContracts.CustomerRes>> SearchCustomers(string text, CancellationToken ct)
    {
        var apiResult = await CustomersClient.SearchByName(text, 5, ct);
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        return apiResult.Value;
    }

    async Task OnValidSubmit()
    {
        _loading = true;
        var result = await SalesClient.Create(new CreateReq(
            _input.Product!.Id,
            _input.Quantity,
            _input.SoldPrice,
            _input.PaymentMethod!.Id,
            _input.Customer?.Id
        ));

        result.Switch(
            _ => NavigationManager.NavigateTo(ReturnUrl ?? "/sales/"),
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
        [Required]
        public ProductContracts.ProductRes? Product { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Required, Range(1, int.MaxValue)]
        public int SoldPrice { get; set; } = 1;

        [Required]
        public PaymentContracts.PaymentMethodRes? PaymentMethod { get; set; }

        public CustomerContracts.CustomerRes? Customer { get; set; }
    }
}