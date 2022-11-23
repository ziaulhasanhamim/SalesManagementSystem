using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MudBlazor.Services;
using Polly;
using Polly.Contrib.WaitAndRetry;
using SalesManagementSystem.Blazor.Services;
using SalesManagementSystem.Contracts.Clients;
using SalesManagementSystem.Server;
using SalesManagementSystem.Server.Endpoints;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastAuthWithEFCore<User, AppDbContext>(options =>
{
    options.GenerateClaims += (claims, u) => claims.Add(
        new Claim(ClaimTypes.Role, u.Role!));
    options.DefaultTokenCreationOptions = new()
    {
        AccessTokenLifeSpan = TimeSpan.FromDays(30)
    };
    options.UseDefaultCredentials(builder.Configuration["SecretKey"]
        ?? throw new ArgumentException("""Set "SecretKey" in appsettings or secrets or env"""));
    builder.Services.AddAuthentication()
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

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration["DbConStr"] ?? throw new ArgumentException("Add 'DbConStr' in environment variable or user secrets")));

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
            },
            Array.Empty<string>()
        }
    });
    options.CustomSchemaIds(t => t.ToString());
});

builder.Services.Configure<SwaggerGeneratorOptions>(
    options => options.InferSecuritySchemes = true);

MapsterConfigurer.Configure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

ProductEndpoints.Map(app);
CustomerEndpoints.Map(app);
PaymentMethodEndpoints.Map(app);
SalesEntryEndpoints.Map(app);
UserEndpoints.Map(app);

app.MapFallbackToFile("index.html");

await AppStartup.BeforeStartup(app);

app.Run();