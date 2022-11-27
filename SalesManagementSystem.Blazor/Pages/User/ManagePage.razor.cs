namespace SalesManagementSystem.Blazor.Pages.User;

using SalesManagementSystem.Contracts.User;

public sealed partial class ManagePage
{
    IReadOnlyList<UserRes> _users = Array.Empty<UserRes>();
    bool _loading = true;

    [Inject]
    public required UsersClient UsersClient { get; set; }

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
        var apiResult = await UsersClient.GetAll();
        if (apiResult.IsFailure)
        {
            throw new Exception(apiResult.Error.Message);
        }
        _users = apiResult.Value;
        _loading = false;
    }

    async Task ShowDeletePromt(UserRes user)
    {
        DialogParameters parameters = new()
        {
            { "ContentText", "Are you sure you want to delete this user?" },
            { "ButtonText", "Delete" },
            { "OkButtonColor", Color.Error }
        };
        var dialogRef = DialogService.Show<Prompt>("Delete user", parameters);
        var result = await dialogRef.Result;
        if (result.Cancelled)
        {
            return;
        }
        var res = await UsersClient.Delete(user.Id);
        if (res.IsFailure)
        {
            throw new Exception(res.Error.Message);
        }
        await LoadData();
    }
}