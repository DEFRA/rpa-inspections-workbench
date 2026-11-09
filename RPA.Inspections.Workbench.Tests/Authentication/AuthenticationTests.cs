using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.Tests.DAL.Mock;

namespace RPA.Inspections.Workbench.Tests.Authentication;

[TestFixture]
public class AuthenticationTests
{
    internal static Dictionary<string, string> Settings() => new()
    {
        ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
        ["AzureAd:TenantId"] = "11111111-1111-1111-1111-111111111111",
        ["AzureAd:ClientId"] = "22222222-2222-2222-2222-222222222222",
        ["AzureAd:ClientSecret"] = "offline-test-only",
        ["Identity:AuditNameClaimType"] = "staff_number",
        ["Authorization:BasicAccessRole"] = "Workbench.Basic",
        ["Authorization:AdminRole"] = "Workbench.Admin",
        ["Authorization:SuperUserRole"] = "Workbench.Super",
        ["Authorization:BcmsRole"] = "Workbench.Bcms",
        ["ConnectionStrings:WorkbenchContext"] = "Server=unused;Database=Workbench;Integrated Security=true",
        ["ConnectionStrings:PeopleContext"] = "Server=unused;Database=People;Integrated Security=true",
        ["ApplyMigrationsOnStartup"] = "false",
        ["AllowedHosts"] = "localhost"
    };

    internal static ServiceProvider Provider()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(Settings()).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(configuration);
        services.AddWorkbenchApplication(configuration);
        return services.BuildServiceProvider();
    }

    internal static ClaimsPrincipal Principal(params string[] roles) => new(new ClaimsIdentity(
        new[] { new Claim("staff_number", "EARTH\\M600300") }
            .Concat(roles.Select(role => new Claim("roles", role))), "Test", "staff_number", "roles"));

    [TestCase(ApplicationServices.BasicAccessPolicy, "Workbench.Basic")]
    [TestCase(ApplicationServices.AdminPolicy, "Workbench.Admin")]
    [TestCase(ApplicationServices.SuperUserPolicy, "Workbench.Super")]
    [TestCase(ApplicationServices.BcmsPolicy, "Workbench.Bcms")]
    public async Task EachPermissionRequiresOnlyItsExplicitRole(string policy, string role)
    {
        using var provider = Provider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        Assert.That((await authorization.AuthorizeAsync(Principal(role), null, policy)).Succeeded, Is.True);
        Assert.That((await authorization.AuthorizeAsync(Principal(), null, policy)).Succeeded, Is.False);
        foreach (var otherRole in new[] { "Workbench.Basic", "Workbench.Admin", "Workbench.Super", "Workbench.Bcms" }.Except(new[] { role }))
            Assert.That((await authorization.AuthorizeAsync(Principal(otherRole), null, policy)).Succeeded, Is.False);
        Assert.That((await authorization.AuthorizeAsync(
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("roles", role) }, null, "staff_number", "roles")),
            null, policy)).Succeeded, Is.False);
    }

    [TestCase("", false)]
    [TestCase("Workbench.Admin", false)]
    [TestCase("Workbench.Super", false)]
    [TestCase("Workbench.Admin,Workbench.Super", true)]
    public async Task BulkUploadRetainsAdminAndSuperUserIntersection(string roles, bool allowed)
    {
        using var provider = Provider();
        var method = typeof(AdminController).GetMethod("Index", new[] { typeof(IFormFile), typeof(bool) });
        var metadata = typeof(AdminController).GetCustomAttributes(true).OfType<IAuthorizeData>()
            .Concat(method.GetCustomAttributes(true).OfType<IAuthorizeData>());
        var policy = await AuthorizationPolicy.CombineAsync(provider.GetRequiredService<IAuthorizationPolicyProvider>(), metadata);
        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(Principal(roles.Split(',', StringSplitOptions.RemoveEmptyEntries)), null, policy);
        Assert.That(result.Succeeded, Is.EqualTo(allowed));
    }

    [Test]
    public async Task HoldingRemainsAuthenticatedOnlyAndIdtTransformationIsGone()
    {
        using var provider = Provider();
        Assert.That(typeof(HoldingController).GetCustomAttributes(true).OfType<IAuthorizeData>(), Is.Empty);
        var fallback = await provider.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        Assert.That((await authorization.AuthorizeAsync(Principal(), null, fallback)).Succeeded, Is.True);
        Assert.That((await authorization.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), null, fallback)).Succeeded, Is.False);
        Assert.That(provider.GetServices<IClaimsTransformation>().Select(t => t.GetType().Name),
            Does.Not.Contain("IdtRoleClaimsTransformation"));
    }

    [TestCase("AzureAd:TenantId", "common")]
    [TestCase("AzureAd:ClientId", "not-a-guid")]
    [TestCase("AzureAd:Instance", "http://identity.invalid")]
    [TestCase("AzureAd:ClientSecret", "")]
    [TestCase("Identity:AuditNameClaimType", "")]
    [TestCase("Authorization:BasicAccessRole", "")]
    [TestCase("Authorization:AdminRole", "CI Inspections Workbench: Admin User")]
    [TestCase("Authorization:SuperUserRole", "Workbench.Admin")]
    [TestCase("Authorization:BcmsRole", "")]
    public void InvalidConfigurationFailsBeforeServingRequests(string key, string value)
    {
        var settings = Settings();
        settings[key] = value;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddWorkbenchApplication(configuration));
    }

    [Test]
    public async Task SchemesCookiesAndRawClaimContractsFollowCphPattern()
    {
        using var provider = Provider();
        var schemes = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.That((await schemes.GetDefaultAuthenticateSchemeAsync()).Name, Is.EqualTo("Cookies"));
        Assert.That((await schemes.GetDefaultChallengeSchemeAsync()).Name, Is.EqualTo("OpenIdConnect"));
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get("OpenIdConnect");
        Assert.That(options.ResponseType, Is.EqualTo("code"));
        Assert.That(options.MapInboundClaims, Is.False);
        Assert.That(((Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler)options.TokenHandler).MapInboundClaims, Is.False);
        Assert.That(options.TokenValidationParameters.RoleClaimType, Is.EqualTo("roles"));
        Assert.That(options.TokenValidationParameters.NameClaimType, Is.EqualTo("staff_number"));
        var cookie = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get("Cookies");
        Assert.That(cookie.Cookie.Name, Is.EqualTo("__Host-Workbench.Auth"));
        Assert.That(cookie.Cookie.SecurePolicy, Is.EqualTo(CookieSecurePolicy.Always));
        Assert.That(cookie.Cookie.HttpOnly, Is.True);
        Assert.That(cookie.ExpireTimeSpan, Is.EqualTo(TimeSpan.FromMinutes(30)));
    }

    [TestCase("EARTH\\MO600300", "m600300")]
    [TestCase("DEMETER\\MO600300", "m600300")]
    [TestCase("earth\\M0600300", "m0600300")]
    [TestCase("M600300", "m600300")]
    [TestCase("OTHER\\M600300", "m600300")]
    public void AuditUsesWorkbenchNormalizationNotCph(string value, string expected)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("staff_number", value) },
            "Test", "staff_number", "roles"));
        Assert.That(AuditIdentity.GetName(principal, "staff_number"), Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("name@example.invalid")]
    [TestCase("Display Name")]
    [TestCase("earth\\")]
    public void InvalidAuditValuesCannotBecomeRequesterIds(string value) =>
        Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("staff_number", value) }, "Test")), "staff_number"));

    [Test]
    public void AuditRejectsMissingDuplicateOrUnauthenticatedClaims()
    {
        Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(Principal(), "missing"));
        Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("staff_number", "M600300") })), "staff_number"));
        var principal = Principal();
        ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("staff_number", "M600400"));
        Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(principal, "staff_number"));
    }

    [Test]
    public void ConfiguredNameClaimStillDrivesPeopleLookupAndOwnership()
    {
        var people = new MockPeopleContext();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = Principal() } };
        var helper = new UserHelper(people.MockContext.Object, accessor);
        Assert.That(helper.CurrentUser(), Is.EqualTo("m600300"));
        Assert.That(helper.CurrentUserEmail(helper.CurrentUser()), Is.EqualTo("Lee.Gordon@Test.com"));
        Assert.That(helper.CurrentUserName(helper.CurrentUser()), Is.EqualTo(people.MockContext.Object.People.Single(p => p.StaffNumber == "M600300").Name));
        Assert.That(helper.GetAllStaffNumbersForCurrentUser(), Is.EquivalentTo(new[] { "M600300" }));
        accessor.HttpContext = null;
        Assert.That(helper.CurrentUser(), Is.EqualTo("Unknown"));
    }
}
