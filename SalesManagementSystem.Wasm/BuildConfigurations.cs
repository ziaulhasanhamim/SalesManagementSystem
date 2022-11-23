using System.Reflection;

namespace SalesManagementSystem.Wasm;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
sealed class BuildConfigurations : Attribute
{
    public static string? BaseUrl { get; private set; }

    public BuildConfigurations(string? baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public static void Load()
    {
        typeof(BuildConfigurations).Assembly.GetCustomAttribute<BuildConfigurations>();
    }
}
