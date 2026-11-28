using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace RPA.Inspections.Workbench.Services;

public static class ApplicationServices
{
    public const string BasicAccessPolicy = "CI Inspections Workbench: Basic Access";
    public const string AdminPolicy = "CI Inspections Workbench: Admin User";
    public const string SuperUserPolicy = "CI Inspections Workbench: Super User";
    public const string BcmsPolicy = "CI Inspections Workbench: BCMS Sev Calc Generator";

    public static IServiceCollection AddWorkbenchApplication(this IServiceCollection services, 
        IConfiguration configuration, IHostEnvironment environment = null)
    {
        var settings = ValidateConfiguration(configuration, environment);

        services.Configure<AuditIdentityOptions>(options => options.AuditNameClaimType = settings.AuditClaim);
        services.AddWorkbenchForwarding(configuration);
        services.AddWorkbenchBlobStorage(configuration);
        services.AddWorkbenchDataProtection(configuration);
        
        AddIdentity(services, configuration, settings.TenantId, settings.Roles, settings.AuditClaim);

        services.AddHttpsRedirection(options => options.HttpsPort = settings.HttpsPort);
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserHelper, UserHelper>();

        AddDatabase(services, settings.WorkbenchConnection, settings.PeopleConnection);

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

        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizePage("/Index", BasicAccessPolicy);
            options.Conventions.AuthorizeFolder("/Admin", AdminPolicy);
            options.Conventions.AuthorizeFolder("/BcmsAdmin", BcmsPolicy);
            options.Conventions.AddPageRoute("/Index", "Home");
        });

        services.AddControllersWithViews();
        services.AddWorkbenchApplicationInsights(configuration);
        
        return services;
    }

    public static string GetAuditName(ClaimsPrincipal principal, string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
            throw new InvalidOperationException("An explicit staff-number audit claim type is required.");
        
        var claims = principal?.FindAll(claimType).ToArray() ?? [];
        
        if (principal?.Identity?.IsAuthenticated != true || claims.Length != 1 || string.IsNullOrWhiteSpace(claims[0].Value))
            throw new InvalidOperationException("The authenticated user must have exactly one nonempty staff-number audit claim.");

        // Preserve Workbench's account normalization, not CPH's different m0 replacement.
        var name = claims[0].Value.ToLowerInvariant()
            .Replace("earth\\mo", "earth\\m")
            .Replace("demeter\\mo", "demeter\\m");
        name = Regex.Replace(name, @".*\\", "");
        
        if (!Regex.IsMatch(name, @"\A[a-z0-9]+\z"))
            throw new InvalidOperationException("The configured audit claim must contain a staff number, not a UPN, email or display name.");
        
        return name;
    }

    private static (Guid TenantId, Dictionary<string, string> Roles, string AuditClaim,
        string WorkbenchConnection, string PeopleConnection, int HttpsPort)
        ValidateConfiguration(IConfiguration configuration, IHostEnvironment environment)
    {
        var tenant = Required(configuration, "AzureAd:TenantId");
        
        if (!Guid.TryParse(tenant, out var tenantId) || tenantId == Guid.Empty)
            throw new InvalidOperationException("AzureAd:TenantId must identify the configured tenant, not a shared or wildcard authority.");
        
        if (!Guid.TryParse(Required(configuration, "AzureAd:ClientId"), out var clientId) || clientId == Guid.Empty)
            throw new InvalidOperationException("AzureAd:ClientId must be an application registration ID.");
        
        if (!Uri.TryCreate(Required(configuration, "AzureAd:Instance"), UriKind.Absolute, out var instance) ||
            instance.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(instance.UserInfo) ||
            !string.IsNullOrEmpty(instance.Query) || !string.IsNullOrEmpty(instance.Fragment))
            throw new InvalidOperationException("AzureAd:Instance must be the externally approved HTTPS identity authority.");
        
        // Supplied by ACA's Key Vault reference in production or the local launcher in development.
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
        ValidateSqlConnection(workbenchConnection, "ConnectionStrings:WorkbenchContext");
        ValidateSqlConnection(peopleConnection, "ConnectionStrings:PeopleContext");

        var allowedHosts = Required(configuration, "AllowedHosts").Split(';');
        if (allowedHosts.Any(host => host.Contains('*') || Uri.CheckHostName(host) == UriHostNameType.Unknown))
            throw new InvalidOperationException("AllowedHosts must contain explicit hostnames separated by semicolons, without wildcards, schemes, paths, ports or empty entries.");

        if (!int.TryParse(configuration["HttpsRedirection:HttpsPort"] ?? "443", out var httpsPort) ||
            httpsPort is < 1 or > 65535)
            throw new InvalidOperationException("HttpsRedirection:HttpsPort must be a valid HTTPS port.");

        return (tenantId, roles, auditClaim, workbenchConnection, peopleConnection, httpsPort);
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration,
        Guid tenantId, Dictionary<string, string> roles, string auditClaim)
    {
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
                    GetAuditName(context.Principal, auditClaim);
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
    }

    private static void AddDatabase(IServiceCollection services, string workbenchConnection, string peopleConnection)
    {
        services.AddDbContext<WorkbenchContext>(o => o.UseSqlServer(workbenchConnection, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<IWorkbenchContext>(provider => provider.GetRequiredService<WorkbenchContext>());
        services.AddDbContext<PeopleContext>(o => o.UseSqlServer(peopleConnection, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<IPeopleContext>(provider => provider.GetRequiredService<PeopleContext>());
        services.AddHealthChecks().AddCheck<SqlReadinessHealthCheck>("sql", tags: ["ready"]);
    }

    private static string Required(IConfiguration configuration, string key) =>
        !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key] :
            throw new InvalidOperationException($"Required external configuration '{key}' is missing.");

    private static string RequiredRole(IConfiguration configuration, string name)
    {
        var role = Required(configuration, "Authorization:" + name);
        
        if (role.Length > 120 || role[0] == '.' || role.Any(c => !IsAppRoleCharacter(c)))
            throw new InvalidOperationException($"Authorization:{name} must be an Entra app-role token value, not its display name.");
        
        return role;
    }

    private static bool IsAppRoleCharacter(char c) =>
        char.IsAsciiLetterOrDigit(c) || ".-_!#$%&'()*+,/:;<=>?@[]^`{|}~".Contains(c);

    private static void ValidateSqlConnection(string connectionString, string key)
    {
        try
        {
            _ = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException($"{key} is not a valid SQL Server connection string.");
        }
    }

    private static void LogAuthenticationFailure(HttpContext context, string code) =>
        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RPA.Inspections.Workbench.Authentication")
            .LogWarning(new EventId(1001, "AuthenticationRejected"), 
                "Authentication rejected. FailureCode: {FailureCode}", code);
}
