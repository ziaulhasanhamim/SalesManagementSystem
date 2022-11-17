using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MudBlazor.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using SalesManagementSystem.Contracts.Clients;
using SalesManagementSystem.Server;
using SalesManagementSystem.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastAuthWithEFCore<User, AppDbContext>(options =>
{
    options.GenerateClaims += (List<Claim> claims, User u) =>
    {
        claims.AddRange(
            u.Roles?.Select(r => new Claim(ClaimTypes.Role, r))
            ?? Array.Empty<Claim>());
    };
    options.DefaultTokenCreationOptions = new()
    {
        AccessTokenLifeSpan = TimeSpan.FromDays(30)
    };
    options.UseDefaultCredentials(builder.Configuration["SecretKey"]
        ?? throw new ArgumentException("""Set "SecretKey" in appsettings or secrets or env"""));
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(jwtOptions =>
        {
            jwtOptions.MapInboundClaims = false;
            jwtOptions.TokenValidationParameters = new()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = options.DefaultTokenCreationOptions.SigningCredentials!.Key
            };
        });
});

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration["DbConStr"] ?? throw new ArgumentException("Add 'DbConStr' in environment variable or user secrets")));

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
            },
            Array.Empty<string>()
        }
    });
    options.CustomSchemaIds(t => t.ToString());
});

builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddHttpClient("SalesManagementServerClient", (sp, client) =>
{
    var configs = sp.GetRequiredService<IConfiguration>();
    var addr = configs["SalesManagementServerAddr"]
        ?? throw new ArgumentNullException("SalesManagementServerAddr", "Set 'SalesManagementServerAddr' in appsettings or env");
    client.BaseAddress = new Uri(addr);
})
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        ClientCertificateOptions = builder.Environment.IsDevelopment() switch
        {
            true => ClientCertificateOption.Manual,
            false => ClientCertificateOption.Automatic
        },
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    })
    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(
        Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(0.5), 4)
    ));

builder.Services.AddScoped(
    sp => new ProductsClient(GetSalesManagementClient(sp)));

builder.Services.AddScoped(
    sp => new CustomersClient(GetSalesManagementClient(sp)));

builder.Services.AddScoped(
    sp => new PaymentMethodsClient(GetSalesManagementClient(sp)));

builder.Services.AddScoped(
    sp => new SalesEntriesClient(GetSalesManagementClient(sp)));

MapsterConfigurer.Configure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

ProductEndpoints.Map(app);
CustomerEndpoints.Map(app);
PaymentMethodEndpoints.Map(app);
SalesEntryEndpoints.Map(app);

app.MapFallbackToPage("/_Host");

app.Run();

static HttpClient GetSalesManagementClient(IServiceProvider sp) =>
    sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("SalesManagementServerClient");