using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Platform probes use HTTP even when the ingress terminates TLS. All application
// routes retain HTTPS redirection and secure authentication cookies.
app.UseWhen(context => context.Request.Path != "/health/live" && context.Request.Path != "/health/ready",
    application => application.UseHttpsRedirection());
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-GB")
    .AddSupportedCultures("en-GB")
    .AddSupportedUICultures("en-GB"));

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") })
    .AllowAnonymous();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
