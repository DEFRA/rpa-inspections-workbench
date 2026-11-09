using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using System.Net;

namespace RPA.Inspections.Workbench.Services;

public static class ApplicationServices
{
    public const string BasicAccessPolicy = "CI Inspections Workbench: Basic Access";
    public const string AdminPolicy = "CI Inspections Workbench: Admin User";
    public const string SuperUserPolicy = "CI Inspections Workbench: Super User";
    public const string BcmsPolicy = "CI Inspections Workbench: BCMS Sev Calc Generator";

    public static IServiceCollection AddWorkbenchApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var tenant = Required(configuration, "AzureAd:TenantId");
        if (!Guid.TryParse(tenant, out var tenantId) || tenantId == Guid.Empty)
            throw new InvalidOperationException("AzureAd:TenantId must identify a single tenant.");
        if (!Guid.TryParse(Required(configuration, "AzureAd:ClientId"), out var clientId) || clientId == Guid.Empty)
            throw new InvalidOperationException("AzureAd:ClientId must be an application registration ID.");
        if (!Uri.TryCreate(Required(configuration, "AzureAd:Instance"), UriKind.Absolute, out var instance) ||
            instance.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(instance.UserInfo) ||
            !string.IsNullOrEmpty(instance.Query) || !string.IsNullOrEmpty(instance.Fragment))
            throw new InvalidOperationException("AzureAd:Instance must be the approved HTTPS identity authority.");
        Required(configuration, "AzureAd:ClientSecret");
        var auditClaim = Required(configuration, "Identity:AuditNameClaimType");
        if (auditClaim.Any(char.IsWhiteSpace))
            throw new InvalidOperationException("Identity:AuditNameClaimType must be an explicit claim type without whitespace.");

        var roles = new Dictionary<string, string>
        {
            [BasicAccessPolicy] = RequiredRole(configuration, "BasicAccessRole"),
            [AdminPolicy] = RequiredRole(configuration, "AdminRole"),
            [SuperUserPolicy] = RequiredRole(configuration, "SuperUserRole"),
            [BcmsPolicy] = RequiredRole(configuration, "BcmsRole")
        };
        if (roles.Values.Distinct(StringComparer.Ordinal).Count() != roles.Count)
            throw new InvalidOperationException("Workbench permissions must map to four distinct Entra app-role values.");

        var workbenchConnection = Required(configuration, "ConnectionStrings:WorkbenchContext");
        var peopleConnection = Required(configuration, "ConnectionStrings:PeopleContext");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

        services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.ResponseType = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectResponseType.Code;
            options.MapInboundClaims = false;
            if (options.TokenHandler is Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler tokenHandler)
                tokenHandler.MapInboundClaims = false;
            options.TokenValidationParameters.RoleClaimType = "roles";
            options.TokenValidationParameters.NameClaimType = auditClaim;

            var previous = options.Events.OnTokenValidated;
            options.Events.OnTokenValidated = async context =>
            {
                if (previous != null) await previous(context);
                if (context.Result?.Failure != null) return;
                var tenants = context.Principal?.FindAll("tid").ToArray() ?? [];
                if (tenants.Length != 1 || !Guid.TryParse(tenants[0].Value, out var actualTenant) || actualTenant != tenantId)
                {
                    LogAuthenticationFailure(context.HttpContext, "TENANT_CONTRACT_REJECTED");
                    context.Fail("The identity does not belong to the configured tenant.");
                    return;
                }
                try
                {
                    AuditIdentity.GetName(context.Principal, auditClaim);
                }
                catch (InvalidOperationException)
                {
                    LogAuthenticationFailure(context.HttpContext, "AUDIT_CONTRACT_REJECTED");
                    context.Fail("The identity is missing its required staff-number audit contract.");
                }
            };
            var previousRemoteFailure = options.Events.OnRemoteFailure;
            options.Events.OnRemoteFailure = async context =>
            {
                LogAuthenticationFailure(context.HttpContext, "OIDC_REMOTE_FAILURE");
                if (previousRemoteFailure != null) await previousRemoteFailure(context);
                if (context.Result?.Handled == true || context.Result?.Skipped == true) return;
                context.HandleResponse();
                context.Response.Redirect("/Account/AuthenticationFailed");
            };
        });
        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "__Host-Workbench.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Path = "/";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
        });
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            foreach (var role in roles)
                options.AddPolicy(role.Key, policy => policy.RequireAuthenticatedUser().RequireRole(role.Value));
        });

        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddScoped<IWorkbenchContext>(_ => new WorkbenchContext(workbenchConnection));
        services.AddScoped<IPeopleContext>(_ => new PeopleContext(peopleConnection));
        services.AddScoped<IUserHelper, UserHelper>();
        services.AddScoped<IFilterService, FilterService>();
        services.AddScoped<IHoldingService, HoldingService>();
        services.AddScoped<ILinkedHoldingService, LinkedHoldingService>();
        services.AddScoped<ICattleService, CattleService>();
        services.AddScoped<IBreedService, BreedService>();
        services.AddScoped<ISendService, SendService>();
        services.AddScoped<IMailService, MailService>();
        services.AddScoped<IPackRequested, PackRequestedService>();
        services.AddScoped<IFileService, FileService>();
        services.AddHttpClient<IHolidayService, HolidayService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            });
        services.AddControllersWithViews();
        return services;
    }

    private static string Required(IConfiguration configuration, string key) =>
        !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key] :
            throw new InvalidOperationException($"Required external configuration '{key}' is missing.");

    private static string RequiredRole(IConfiguration configuration, string name)
    {
        var role = Required(configuration, "Authorization:" + name);
        if (role.Length > 120 || role[0] == '.' ||
            role.Any(c => !char.IsAsciiLetterOrDigit(c) && !".-_!#$%&'()*+,/:;<=>?@[]^`{|}~".Contains(c)))
            throw new InvalidOperationException($"Authorization:{name} must be an Entra app-role token value, not its display name.");
        return role;
    }

    private static void LogAuthenticationFailure(HttpContext context, string code) =>
        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RPA.Inspections.Workbench.Authentication")
            .LogWarning(new EventId(1001, "AuthenticationRejected"), "Authentication rejected. FailureCode: {FailureCode}", code);
}
