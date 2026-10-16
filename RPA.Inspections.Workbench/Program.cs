using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.IIS;
using RPA.Inspections.Workbench;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Migrations;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using System.Data.Entity.Migrations;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var workbenchConnectionString = builder.Configuration.GetConnectionString("WorkbenchContext")
    ?? throw new InvalidOperationException("Connection string 'WorkbenchContext' is not configured.");
var peopleConnectionString = builder.Configuration.GetConnectionString("PeopleContext")
    ?? throw new InvalidOperationException("Connection string 'PeopleContext' is not configured.");

// IIS in-process hosting authenticates the user itself; Kestrel needs the Negotiate handler.
var hostedInIis = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APP_POOL_ID"));
if (hostedInIis)
{
    builder.Services.AddAuthentication(IISServerDefaults.AuthenticationScheme);
}
else
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IClaimsTransformation, IdtRoleClaimsTransformation>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IWorkbenchContext>(_ => new WorkbenchContext(workbenchConnectionString));
builder.Services.AddScoped<IPeopleContext>(_ => new PeopleContext(peopleConnectionString));

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IFilterService, FilterService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ILinkedHoldingService, LinkedHoldingService>();
builder.Services.AddScoped<ICattleService, CattleService>();
builder.Services.AddScoped<IBreedService, BreedService>();
builder.Services.AddScoped<ISendService, SendService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IPackRequested, PackRequestedService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddHttpClient<IHolidayService, HolidayService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // The holiday service is reached through the corporate proxy with the application pool identity
        DefaultProxyCredentials = CredentialCache.DefaultCredentials
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Schema changes are normally applied by the deployment pipeline; this switch keeps the previous
// "migrate on start-up" behaviour available for environments that still need it.
if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    new DbMigrator(new Configuration(workbenchConnectionString)).Update();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB")
    .AddSupportedCultures("en-GB")
    .AddSupportedUICultures("en-GB"));

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
