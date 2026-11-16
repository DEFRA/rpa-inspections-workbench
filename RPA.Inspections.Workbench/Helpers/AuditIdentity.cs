using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace RPA.Inspections.Workbench.Helpers;

public static class AuditIdentity
{
    public static string GetName(ClaimsPrincipal principal, string auditClaimType)
    {
        if (string.IsNullOrWhiteSpace(auditClaimType))
            throw new InvalidOperationException("An explicit staff-number audit claim type is required.");
        
        var claims = principal?.FindAll(auditClaimType).ToArray() ?? [];
        
        if (principal?.Identity?.IsAuthenticated != true || claims.Length != 1 || string.IsNullOrWhiteSpace(claims[0].Value))
            throw new InvalidOperationException("The authenticated user must have exactly one nonempty staff-number audit claim.");

        return claims[0].Value;
    }
}
