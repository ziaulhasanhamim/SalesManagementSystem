namespace SalesManagementSystem.Blazor.Pages.User;

using SalesManagementSystem.Contracts.User;

public sealed partial class ManagePage
{
    IReadOnlyList<UserRes> _users = Array.Empty<UserRes>();
    bool _loading = true;

    [Inject]
    public required UsersClient UsersClient { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        await base.OnInitializedAsync();
    }

    async Task LoadData()
    {
        _loading = true;
        StateHasChanged();
        var apiResult = await UsersClient.GetAll();
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _users = apiResult.Value;
        _loading = false;
    }
}