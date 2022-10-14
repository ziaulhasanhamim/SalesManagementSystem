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

        TypeAdapterConfig<PaymentMethod, string>.NewConfig()
            .MapWith(p => p.Name);

        _configured = true;
    }
}