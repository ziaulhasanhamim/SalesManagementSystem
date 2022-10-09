namespace SalesManagementSystem.Server;

public static class MapsterConfigurer
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
        {
            return;
        }
        TypeAdapterConfig.GlobalSettings.Default.ShallowCopyForSameType(true);
        _configured = true;
    }
}