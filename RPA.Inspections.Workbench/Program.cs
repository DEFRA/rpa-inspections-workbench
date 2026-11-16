using RPA.Inspections.Workbench.Services;

var builder = WebApplication.CreateBuilder(args);

// Database schema changes are reviewed deployment operations, never startup actions.
if (builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    throw new InvalidOperationException(
        "ApplyMigrationsOnStartup is no longer supported. Follow the existing-schema deployment procedure in README.md.");
}

builder.Configuration.AddWorkbenchKeyVault(builder.Environment);
builder.Services.AddWorkbenchApplication(builder.Configuration);

var app = builder.Build();

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
