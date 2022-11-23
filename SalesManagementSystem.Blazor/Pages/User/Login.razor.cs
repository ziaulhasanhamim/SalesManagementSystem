namespace SalesManagementSystem.Blazor.Pages.User;

using SalesManagementSystem.Contracts.User;

public sealed partial class Login
{
    readonly InputModel _input = new();
    EditContext _formCtx = null!;
    ValidationMessageStore _messageStore = null!;
    bool _loading;

    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [Inject]
    public required UsersClient UsersClient { get; init; }

    [Inject]
    public required IAuthService AuthService { get; init; }

    [Inject]
    public required IJSRuntime JSRuntime { get; init; }

    [Inject]
    public required NavigationManager NavigationManager { get; init; }

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
        var result = await UsersClient.Login(new LoginReq(
            _input.Email,
            _input.Password
        ));

        if (result.IsFailure)
        {
            if (result is { Error: ValidationErrorRes { Errors: var errsDict } })
            {
                AddErrsToStore(errsDict);
                _formCtx.NotifyValidationStateChanged();
                _loading = false;
                return;
            }
            throw new Exception(result.Error.Message);
        }
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "auth", result.Value.Token);
        await AuthService.Authorize();
        NavigationManager.NavigateTo(ReturnUrl ?? "/products/");
        _loading = false;
    }

    void AddErrsToStore(IDictionary<string, IEnumerable<string>> errsDict)
    {
        foreach (var errsKeyVal in errsDict)
        {
            if (errsKeyVal.Key == "User")
            {
                _messageStore.Add(
                    FieldIdentifier.Create(() => _input.Email),
                    errsKeyVal.Value);
            }
            _messageStore.Add(
                new FieldIdentifier(_input, errsKeyVal.Key),
                errsKeyVal.Value);
        }
    }

    public sealed class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}