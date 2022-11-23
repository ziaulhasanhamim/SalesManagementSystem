namespace SalesManagementSystem.Blazor.Pages.User;

using SalesManagementSystem.Contracts.User;

public sealed partial class AddPage
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
        var result = await UsersClient.Create(new CreateReq(
            _input.Email,
            _input.Password,
            _input.Role
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
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(8)]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";
    }
}