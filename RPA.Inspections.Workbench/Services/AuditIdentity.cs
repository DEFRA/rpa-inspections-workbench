using System.Security.Claims;
using System.Text.RegularExpressions;

namespace RPA.Inspections.Workbench.Services;

public static class AuditIdentity
{
    public static string GetName(ClaimsPrincipal principal, string claimType)
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
}
