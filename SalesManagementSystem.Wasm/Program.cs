using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http;
using MudBlazor.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using SalesManagementSystem.Blazor;
using SalesManagementSystem.Blazor.Services;
using SalesManagementSystem.Contracts.Clients;
using SalesManagementSystem.Wasm;

BuildConfigurations.Load();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

addHttpClient();

builder.Services.AddSingleton<SalesEntriesClient>();
builder.Services.AddSingleton<ProductsClient>();
builder.Services.AddSingleton<PaymentMethodsClient>();
builder.Services.AddSingleton<CustomersClient>();
builder.Services.AddSingleton<UsersClient>();

builder.Services.AddMudServices();

builder.Services.AddScoped<IAuthService, JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => (AuthenticationStateProvider)sp.GetRequiredService<IAuthService>());
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();

void addHttpClient()
{
    builder.Services.AddSingleton(_ =>
    {
        var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(
            Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(0.5), 4)
        );

        var handler = new PolicyHttpMessageHandler(retryPolicy)
        {
            InnerHandler = new HttpClientHandler()
        };
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new(BuildConfigurations.BaseUrl ?? builder.HostEnvironment.BaseAddress)
        };
        return httpClient;
    });
}
