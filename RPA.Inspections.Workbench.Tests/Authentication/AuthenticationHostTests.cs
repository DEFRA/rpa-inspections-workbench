using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NUnit.Framework;

namespace RPA.Inspections.Workbench.Tests.Authentication;

[TestFixture]
public class AuthenticationHostTests
{
    private sealed class OfflineApplication : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("https_port", "443");
            foreach (var setting in AuthenticationTests.Settings())
                builder.UseSetting(setting.Key, setting.Value);
            builder.ConfigureServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                services.AddControllersWithViews().AddApplicationPart(typeof(AuthenticationSessionController).Assembly);
                services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                        new OpenIdConnectConfiguration
                        {
                            Issuer = "https://identity.invalid/tenant/v2.0",
                            AuthorizationEndpoint = "https://identity.invalid/authorize",
                            TokenEndpoint = "https://identity.invalid/token"
                        }));
            });
        }

        public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    [Test]
    public async Task AnonymousHomeAndHoldingChallengeOidc()
    {
        using var application = new OfflineApplication();
        using var client = application.Client();
        foreach (var path in new[] { "/", "/Holding" })
        {
            var response = await client.GetAsync(path);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(response.Headers.Location.Host, Is.EqualTo("identity.invalid"));
            Assert.That(response.Headers.Location.Query, Does.Contain("response_type=code"));
        }
    }

    [TestCase("/Account/AccessDenied", HttpStatusCode.Forbidden)]
    [TestCase("/Account/AuthenticationFailed", HttpStatusCode.Unauthorized)]
    public async Task FailureEndpointsAreAnonymousAndDoNotLoop(string path, HttpStatusCode expected)
    {
        using var application = new OfflineApplication();
        using var client = application.Client();
        var response = await client.GetAsync(path);
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(response.Headers.Location, Is.Null);
    }

    [TestCase("Workbench.Basic", "/Admin")]
    [TestCase("Workbench.Super", "/Admin")]
    [TestCase("Workbench.Admin", "/BcmsAdmin")]
    [TestCase("Workbench.Bcms", "/")]
    [TestCase("", "/")]
    public async Task WrongPermissionIsForbiddenByRealMvcPipeline(string roles, string path)
    {
        using var application = new OfflineApplication();
        using var client = application.Client();
        var signIn = await client.GetAsync("/__test/session?roles=" + Uri.EscapeDataString(roles));
        Assert.That(signIn.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(signIn.Headers.GetValues("Set-Cookie").Single(), Does.Contain("__Host-Workbench.Auth=")
            .And.Contain("secure").And.Contain("httponly"));
        var response = await client.GetAsync(path);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location.AbsolutePath, Is.EqualTo("/Account/AccessDenied"));
        var denied = await client.GetAsync(response.Headers.Location);
        Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [TestCase("Workbench.Basic", false, false)]
    [TestCase("Workbench.Basic,Workbench.Admin", true, false)]
    [TestCase("Workbench.Basic,Workbench.Bcms", false, true)]
    public async Task HomeRendersNavigationUsingConfiguredPermissionPolicies(string roles, bool admin, bool bcms)
    {
        using var application = new OfflineApplication();
        using var client = application.Client();
        await client.GetAsync("/__test/session?roles=" + Uri.EscapeDataString(roles));
        var response = await client.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html.Contains("location.href='/Admin'"), Is.EqualTo(admin));
        Assert.That(html.Contains("location.href='/BcmsAdmin'"), Is.EqualTo(bcms));
    }

    [TestCase("Workbench.Admin", HttpStatusCode.Redirect)]
    [TestCase("Workbench.Super", HttpStatusCode.Redirect)]
    [TestCase("Workbench.Admin,Workbench.Super", HttpStatusCode.BadRequest)]
    public async Task BulkUploadRequiresBothRolesBeforeAntiforgeryValidation(string roles, HttpStatusCode expected)
    {
        using var application = new OfflineApplication();
        using var client = application.Client();
        await client.GetAsync("/__test/session?roles=" + Uri.EscapeDataString(roles));
        var response = await client.PostAsync("/Admin", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        if (expected == HttpStatusCode.Redirect)
            Assert.That(response.Headers.Location.AbsolutePath, Is.EqualTo("/Account/AccessDenied"));
    }

    [Test]
    public async Task RemoteFailureRedirectsToSafeAnonymousEndpoint()
    {
        using var application = new OfflineApplication();
        using var client = application.Client();
        var response = await client.GetAsync("/signin-oidc?error=access_denied");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location.ToString(), Is.EqualTo("/Account/AuthenticationFailed"));
    }
}

// Only registered by the offline factory; production never loads this application part.
[AllowAnonymous]
public class AuthenticationSessionController : Controller
{
    [HttpGet("/__test/session")]
    public async Task<IActionResult> SignIn(string roles = "")
    {
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            AuthenticationTests.Principal((roles ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)));
        return NoContent();
    }
}
