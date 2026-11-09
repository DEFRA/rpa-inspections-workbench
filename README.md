# Inspections Workbench

ASP.NET Core MVC (.NET 10), using Entity Framework 6 for Workbench and People data.

## Authentication and permissions

`Program.cs` registers `Services/ApplicationServices.cs`, following CPH Non Subsidy's
Microsoft.Identity.Web pattern: single-tenant Entra ID, authorization-code OIDC sign-in,
secure cookie sessions, unmapped token claims, and explicit named authorization policies.
Windows/Negotiate authentication and IDT SQL role loading are no longer used.
**No IDT roles are read, translated, or automatically migrated.** Historical assessment
reports under `docs/` and `reports/` describe the old application, not current runtime configuration.

Supply these settings externally (environment variables use `__` instead of `:`):

| Setting | Contract |
| --- | --- |
| `AzureAd:Instance` | Approved HTTPS authority; defaults to `https://login.microsoftonline.com/` |
| `AzureAd:TenantId` | Single tenant GUID, not `common` or `organizations` |
| `AzureAd:ClientId` | Workbench application registration GUID |
| `AzureAd:ClientSecret` | Secret from the deployment secret store; never commit it |
| `Identity:AuditNameClaimType` | Exact raw token claim type supplying the staff number; see below |
| `Authorization:BasicAccessRole` | Entra app-role **value** required by Home |
| `Authorization:AdminRole` | Distinct app-role value required by all Admin actions |
| `Authorization:SuperUserRole` | Distinct additional app-role value required by Admin's bulk-upload POST |
| `Authorization:BcmsRole` | Distinct app-role value required by BCMS actions |
| `ConnectionStrings:WorkbenchContext` | Existing Workbench SQL connection |
| `ConnectionStrings:PeopleContext` | Existing People SQL connection |

There are deliberately no default role mappings. The identity/application owners must
create or approve four distinct app-role values and assign users/groups to them before
deployment. Values cannot be legacy display names containing spaces. No permission
hierarchy is inferred: Super User alone does **not** grant Admin, Admin does not imply
Basic Access or BCMS, and Basic Access does not grant Admin. As before, bulk upload
requires **both** Admin and Super User. Holding routes retain authenticated-only access;
the fallback policy also requires authentication for other unannotated application routes.
Navigation uses the same policies as the corresponding controllers.

### Staff-number audit contract

Configure an Entra claim that actually contains the user's People `StaffNumber` or the
equivalent existing domain-qualified account name. Do **not** select `name`, email,
`preferred_username`/UPN or `oid` as a convenient replacement for staff numbers.
Those are not a People/audit mapping. Configure the required claim issuance at the identity
provider and verify representative users against People and historical audit records.
Sign-in fails closed if the configured claim is missing, duplicated, empty, or cannot
normalize to an alphanumeric staff number. There is no email/display-name/ID fallback.

Workbench's existing normalization is retained: lowercase, replace `earth\mo` with
`earth\m` and `demeter\mo` with `demeter\m`, then strip the domain prefix.
It does **not** use CPH's different `m0` replacement. The normalized staff number continues
to drive People email/name lookups and pack ownership. Pack request records still uppercase
the requester ID. The no-HTTP-context helper behavior remains `Unknown`.

### Deployment prerequisites

- Register the externally visible HTTPS callback URI ending in `/signin-oidc`.
- Enable anonymous access at IIS/reverse-proxy level and disable Windows authentication
  for this app; authorization is now handled inside the app, not by IIS Windows challenge.
- Provide the tenant, client credentials, approved app-role mappings and staff-number
  claim issuance above. Blank required settings intentionally prevent startup.
- Serve the app over HTTPS. Behind TLS termination, configure trusted forwarded headers
  at the hosting layer so the app sees the original HTTPS scheme; do not trust arbitrary
  clients' forwarded headers. HTTP requests are redirected to HTTPS.
- Persist/share ASP.NET Core data-protection keys securely across restarts/replicas.
  The new `__Host-Workbench.Auth` secure, HttpOnly cookie has a 30-minute sliding session.
  IDT sessions are not reused; role assignment changes require a new sign-in/session expiry.
- Retain SQL, holiday-service proxy and SMTP access for the existing runtime identity.
  Entra interactive sign-in does not migrate Integrated Security database connections,
  application pool credentials, pickup directory access, or People data.
- Schema migrations remain deployment-managed unless `ApplyMigrationsOnStartup` is enabled.

Forbidden requests redirect to `/Account/AccessDenied` (403); OIDC failures redirect to
`/Account/AuthenticationFailed` (401). These anonymous endpoints do not expose exceptions,
tokens or identity data. Authentication failure logging records only a failure code.

## Build and tests

From this directory:

```powershell
dotnet build .\RPA.Inspections.Workbench\RPA.Inspections.Workbench.csproj
dotnet test .\RPA.Inspections.Workbench.Tests\RPA.Inspections.Workbench.Tests.csproj --filter "FullyQualifiedName~Authentication"
```

The authentication tests use local signing keys/static OIDC metadata and an offline test
host; they do not require Entra, IDT, SQL, SMTP, or the holiday service. A real tenant sign-in
and representative staff-number/permission acceptance check are still required before go-live.
