using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using SalesManagementSystem.Server;
using SalesManagementSystem.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration["DbConStr"] ?? throw new ArgumentException("Add 'DbConStr' in environment variable or user secrets")));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(t => t.ToString()));
builder.Services.AddMudServices();

MapsterConfigurer.Configure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
