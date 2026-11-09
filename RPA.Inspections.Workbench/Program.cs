using RPA.Inspections.Workbench.Migrations;
using RPA.Inspections.Workbench.Services;
using System.Data.Entity.Migrations;

var builder = WebApplication.CreateBuilder(args);

var workbenchConnectionString = builder.Configuration.GetConnectionString("WorkbenchContext")
    ?? throw new InvalidOperationException("Connection string 'WorkbenchContext' is not configured.");
builder.Services.AddWorkbenchApplication(builder.Configuration);

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

app.UseHttpsRedirection();
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
