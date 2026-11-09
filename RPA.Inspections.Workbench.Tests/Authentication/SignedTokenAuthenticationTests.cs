using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Validators;
using NUnit.Framework;
using RPA.Inspections.Workbench.Services;

namespace RPA.Inspections.Workbench.Tests.Authentication;

[TestFixture]
public class SignedTokenAuthenticationTests
{
    [TestCase("valid", true)]
    [TestCase("missing-audit", false)]
    [TestCase("duplicate-audit", false)]
    [TestCase("upn-audit", false)]
    [TestCase("foreign-tenant", false)]
    [TestCase("duplicate-tenant", false)]
    [TestCase("missing-tenant", false)]
    [TestCase("wrong-audience", false)]
    [TestCase("wrong-signature", false)]
    [TestCase("expired", false)]
    public async Task SignedTokensUseRealMiwValidationAndWorkbenchIdentityContract(string scenario, bool accepted)
    {
        using var provider = AuthenticationTests.Provider();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get("OpenIdConnect");
        var settings = AuthenticationTests.Settings();
        var tenant = settings["AzureAd:TenantId"];
        var client = settings["AzureAd:ClientId"];
        var issuer = $"https://login.microsoftonline.com/{tenant}/v2.0";
        using var rsa = RSA.Create(2048);
        using var otherRsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa) { KeyId = "offline-key" };
        var metadata = new OpenIdConnectConfiguration { Issuer = issuer };
        metadata.SigningKeys.Add(key);
        options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(metadata);
        using var backchannel = new HttpClient(new RejectNetwork());
        options.Backchannel = backchannel;
        var issuerValidator = AadIssuerValidator.GetAadIssuerValidator(options.Authority, backchannel,
            _ => (BaseConfigurationManager)options.ConfigurationManager);
        IssuerValidator offlineIssuerValidation = issuerValidator.Validate;
        Assert.That(options.TokenValidationParameters.IssuerValidator.Method, Is.EqualTo(offlineIssuerValidation.Method));
        options.TokenValidationParameters.IssuerValidator = offlineIssuerValidation;

        var claims = new List<Claim>
        {
            new("tid", tenant),
            new("oid", "33333333-3333-3333-3333-333333333333"),
            new("sub", "offline-user"),
            new("ver", "2.0"),
            new("staff_number", "EARTH\\MO600300"),
            new("roles", "Workbench.Basic")
        };
        switch (scenario)
        {
            case "missing-audit": claims.RemoveAll(c => c.Type == "staff_number"); break;
            case "duplicate-audit": claims.Add(new Claim("staff_number", "M600400")); break;
            case "upn-audit":
                claims.RemoveAll(c => c.Type == "staff_number");
                claims.Add(new Claim("staff_number", "someone@example.invalid"));
                break;
            case "foreign-tenant":
                claims.RemoveAll(c => c.Type == "tid");
                const string foreignTenant = "44444444-4444-4444-4444-444444444444";
                claims.Add(new Claim("tid", foreignTenant));
                issuer = $"https://login.microsoftonline.com/{foreignTenant}/v2.0";
                break;
            case "duplicate-tenant": claims.Add(new Claim("tid", tenant)); break;
            case "missing-tenant": claims.RemoveAll(c => c.Type == "tid"); break;
        }
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = scenario == "wrong-audience" ? "55555555-5555-5555-5555-555555555555" : client,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(
                scenario == "wrong-signature" ? new RsaSecurityKey(otherRsa) { KeyId = "offline-key" } : key,
                SecurityAlgorithms.RsaSha256),
            IssuedAt = DateTime.UtcNow.AddMinutes(-20),
            NotBefore = DateTime.UtcNow.AddMinutes(-20),
            Expires = DateTime.UtcNow.AddMinutes(scenario == "expired" ? -10 : 5)
        });
        var parameters = options.TokenValidationParameters.Clone();
        parameters.ConfigurationManager = (BaseConfigurationManager)options.ConfigurationManager;
        parameters.ValidAudience = client;
        parameters.ValidIssuer = "https://login.microsoftonline.com/{tenantid}/v2.0";
        var validation = await options.TokenHandler.ValidateTokenAsync(token, parameters);
        TokenValidatedContext context = null;
        if (validation.IsValid)
        {
            context = new TokenValidatedContext(new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme("OpenIdConnect", null, typeof(OpenIdConnectHandler)), options,
                new ClaimsPrincipal(validation.ClaimsIdentity), new AuthenticationProperties())
            {
                SecurityToken = new JwtSecurityToken(token)
            };
            await options.Events.OnTokenValidated(context);
        }
        Assert.That(validation.IsValid && context?.Result?.Failure == null, Is.EqualTo(accepted));
        if (scenario == "foreign-tenant")
        {
            Assert.That(validation.IsValid, Is.True, "MIW accepts the signed tenant/issuer template; Workbench must still enforce its configured tenant.");
            Assert.That(context.Result.Failure, Is.Not.Null);
        }
        if (accepted)
        {
            Assert.That(context.Principal.IsInRole("Workbench.Basic"), Is.True);
            Assert.That(context.Principal.HasClaim(c => c.Type == ClaimTypes.Role), Is.False);
            Assert.That(context.Principal.Identity.Name, Is.EqualTo("EARTH\\MO600300"));
            Assert.That(AuditIdentity.GetName(context.Principal, "staff_number"), Is.EqualTo("m600300"));
        }
    }

    private sealed class RejectNetwork : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Network access is forbidden in offline authentication tests.");
    }
}
