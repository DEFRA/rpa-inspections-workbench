# Inspections Workbench

ASP.NET Core MVC (.NET 10), using **Entity Framework 6.4.4** and its
`System.Data.SqlClient` SQL Server provider for Workbench and People data.
This intentionally differs from CPH's EF Core version. The web framework,
authentication and Key Vault integration remain unchanged.

## Authentication and permissions

`Program.cs` registers `Services/ApplicationServices.cs`, following CPH Non Subsidy's
Microsoft.Identity.Web pattern: single-tenant Entra ID, authorization-code OIDC sign-in,
secure cookie sessions, unmapped token claims, and explicit named authorization policies.
Registration is split into configuration validation, identity setup, and database setup,
following CPH's structure while retaining Workbench's MVC and business-service registrations.
Windows/Negotiate authentication and IDT SQL role loading are no longer used.
**No IDT roles are read, translated, or automatically migrated.** Historical assessment
reports under `docs/` describe the old application, not current runtime configuration.
The app-local `reports/rpa-inspections-workbench.md` records current source and verification gaps.

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
| `ConnectionStrings:WorkbenchContext` | Existing Workbench SQL connection, loaded from Key Vault |
| `ConnectionStrings:PeopleContext` | Existing People SQL connection, loaded from Key Vault |
| `KeyVault:VaultUri` | HTTPS vault endpoint, supplied externally for Key Vault loading |
| `KeyVault:ManagedIdentityClientId` | Optional user-assigned managed identity client GUID; omit for system-assigned identity |
| `BlobStorage:ServiceUri` | Required HTTPS Blob service endpoint, for example `https://<account>.blob.core.windows.net/` |
| `BlobStorage:ContainerName` | Existing private container; defaults to `workbench-files` |
| `BlobStorage:ManagedIdentityClientId` | Optional user-assigned managed identity client GUID; omit for system-assigned identity |
| `AllowedHosts` | Explicit hostnames separated by semicolons; defaults to `localhost` for local use. Override for deployment; no wildcards, schemes, ports or paths |
| `HttpsRedirection:HttpsPort` | HTTPS redirect port from 1 to 65535; defaults to `443`. Set the local HTTPS port when applicable |

Configuration is validated before application services are registered. Both SQL connection
strings must parse as System.Data.SqlClient connection strings; this does not verify
database connectivity or credentials. Invalid hostnames or HTTPS ports also prevent startup.

### Key Vault configuration

`Program.cs` loads the Azure Key Vault configuration provider before calling
`AddWorkbenchApplication`. Connection strings are no longer stored in any checked-in
application settings file. Configure a separate vault for each environment and create:

| Secret name | Configuration key |
| --- | --- |
| `ConnectionStrings--WorkbenchContext` | `ConnectionStrings:WorkbenchContext` |
| `ConnectionStrings--PeopleContext` | `ConnectionStrings:PeopleContext` |
| `AzureAd--ClientSecret` (optional if supplied by another external secret source) | `AzureAd:ClientSecret` |

Use the approved SQL connection strings as the secret values; do not put them in source
control, command history, logs or support messages. Double hyphens map to configuration
section separators. The provider loads enabled secrets from the vault, so use an
application/environment-specific vault rather than a shared vault containing unrelated secrets.

Set the non-secret bootstrap configuration externally:

```powershell
$env:KeyVault__VaultUri = "https://<vault-name>.vault.azure.net/"
# Only for a user-assigned managed identity:
$env:KeyVault__ManagedIdentityClientId = "<managed-identity-client-guid>"
```

In Azure, enable the application's system-assigned identity or attach the configured
user-assigned identity. Outside Development, the application uses `ManagedIdentityCredential`
without falling back to developer accounts or client-secret credentials. Grant vault-scoped
secret list/get access (for RBAC, `Key Vault Reader` and `Key Vault Secrets User`; for legacy
access policies, secret `Get` and `List`). Allow the host through the vault firewall/private
endpoint and DNS configuration. This change does not provision a vault, assign roles or
copy secrets into Azure.

In Development, a configured vault uses `DefaultAzureCredential`, which can use an approved
Azure CLI/Visual Studio developer sign-in with access to a non-production vault. If no vault
URI is configured, the provider is not added: local/offline runs may supply
`ConnectionStrings__WorkbenchContext` and `ConnectionStrings__PeopleContext` externally.
Required-setting validation still rejects missing connection strings. For deployed Key Vault
integration, always set `KeyVault__VaultUri`; leaving it blank does not enable Key Vault.

When configured, Key Vault is added last and overrides earlier configuration sources.
Both SQL secrets must exist in the vault and be nonempty even if earlier sources provide
connection strings. Authentication, access, network and secret-loading errors stop startup;
there is no fallback after a vault failure. Secret values are not logged. Secrets load at
startup; restart the application after rotating them.

Moving a SQL connection string into Key Vault does not change SQL authentication. In
particular, `Integrated Security` still requires the existing SQL-capable runtime identity.

Reference: [ASP.NET Core Azure Key Vault configuration provider](https://learn.microsoft.com/aspnet/core/security/key-vault-configuration?view=aspnetcore-10.0).

### Blob Storage configuration

Generated inspection workbooks and BCMS report downloads use `Azure.Storage.Blobs`
12.30.1 through the shared `IFileStorage` service. There is no local-file fallback.
Configure the non-secret settings externally, or use equivalent `BlobStorage--...`
Key Vault entries:

```powershell
$env:BlobStorage__ServiceUri = "https://<account>.blob.core.windows.net/"
$env:BlobStorage__ContainerName = "workbench-files"
# Only for a user-assigned managed identity:
$env:BlobStorage__ManagedIdentityClientId = "<managed-identity-client-guid>"
```

Provision the container privately with anonymous blob access disabled. Assign the
application's managed identity **Storage Blob Data Contributor** scoped to this
container, and allow the application's network/private DNS through the storage firewall.
Control-plane Contributor alone does not grant blob data access. Outside Development,
the app uses managed identity only; Development uses `DefaultAzureCredential` with an
approved developer account. No storage account keys, connection strings or SAS tokens
are accepted by this integration. The app does not create containers or assign roles.
Missing/invalid endpoint, container or managed-identity settings fail startup. Access
permissions and connectivity must be verified against the deployed storage account.

- Spreadsheet imports are parsed in memory and discarded, matching their previous
  temporary-file lifetime. They no longer write files under `Uploads` and are not
  retained as source-upload blobs.
- The read-only `Uploads/CII Animal Data Spreadsheet v2.2.xlsm` template remains
  packaged with the app. Historical generated packs and unused files in that source
  folder are not copied into new build/publish output.
- Pack generation saves the macro-enabled workbook into memory and uploads it before
  recording completion or sending the notification. Unique blob names take the form
  `packs/<pack-request-guid>/<generation-guid>/CII Animal Data Spreadsheet<safe-cph>.xlsm`.
  The existing `PackRequested.savePath` column stores this relative blob name.
  Uploads cannot overwrite an existing blob.
- Downloads retain the existing authenticated/controller-policy and database-record
  checks, then stream the blob through the application with the friendly file name.
  No public blob URL or SAS token is exposed to the browser. Missing files return 404;
  other storage failures are logged and propagated rather than returning an empty file.
- Deletion removes the recorded blob before hiding the request in SQL. An already
  absent blob is treated as an idempotent deletion. Storage failures leave the request
  visible for retry. Blob/SQL operations are not a distributed transaction; uploads
  followed by a failed SQL save can leave an orphan blob, and a failed SQL update after
  deletion requires retry. Configure an owner-approved retention/cleanup policy.
- BCMS HTML report generation is external to this repository. Update that producer to
  upload private `reports/<request-guid>/<filename>.html` blobs and store the relative
  name in `BcmsRequested.Filepath`. `open_report` streams only recorded reports and
  retains the existing BCMS authorization policy. Producer deployment is a prerequisite.
- Expiry and bulk-clear operations retain their existing database behavior; they do
  not automatically purge blobs. Apply an approved storage lifecycle policy rather
  than guessing a retention period in application code.

**Existing-file rollout is mandatory:** inventory recorded pack/report paths, copy each
approved existing file into the appropriate `packs/` or `reports/` key, verify content
and access, then update the corresponding database reference through an approved
deployment operation. Use unique request-scoped keys, not only basenames, to avoid
collisions. Absolute Windows/UNC paths and arbitrary URLs are rejected. Do not switch
traffic until the external report producer and existing file references are ready.
No live files, SQL rows, containers or role assignments are migrated by this code change.

Reference: [Upload blobs with the Azure SDK for .NET](https://learn.microsoft.com/azure/storage/blobs/storage-blob-upload).

Blob integration verification on 2026-11-10: **288 passed, 3 skipped, 0 failed**
in the full Debug suite (291 total). Release publish succeeded and the published
`Uploads` folder contained only the active template. Tests verify workbook/macro
preservation, unique blob names, no runtime local-file writes, streamed downloads,
configuration validation and storage-failure behavior using mocked Azure SDK clients.
Live Azure storage access, RBAC/network configuration and legacy-file migration
have not been verified or performed.

### Role mapping

There are deliberately no default role mappings. The identity/application owners must
create or approve four distinct app-role values and assign users/groups to them before
deployment. Values cannot be legacy display names containing spaces. No permission
hierarchy is inferred: Super User alone does **not** grant Admin, Admin does not imply
Basic Access or BCMS, and Basic Access does not grant Admin. As before, bulk upload
requires **both** Admin and Super User. Holding routes retain authenticated-only access;
the fallback policy also requires authentication for other unannotated application routes.
Navigation uses the same policies as the corresponding controllers.

### Staff-number audit contract

Validation and normalization live in `ApplicationServices.GetAuditName`, shared by
sign-in validation and `UserHelper.CurrentUser`.

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
- Schema adoption/migrations are deployment-managed. Leave `ApplyMigrationsOnStartup=false`;
  setting it to true now fails startup rather than replaying EF6 migrations.

Forbidden requests redirect to `/Account/AccessDenied` (403); OIDC failures redirect to
`/Account/AuthenticationFailed` (401). These anonymous endpoints do not expose exceptions,
tokens or identity data. Authentication failure logging records only a failure code.

## Entity Framework 6.4.4 and existing databases

The application references `EntityFramework` **6.4.4**, not EF Core. Both contexts use
EF6 `DbContext`/`DbSet`, fluent mappings, built-in lazy-loading proxies and automatic
DataAnnotations validation. Validation failures on save use EF6's
`DbEntityValidationException`. Normal change detection remains enabled for tracked
edits; disconnected updates continue to use `SetModified`.

Both contexts are scoped services, and their interfaces resolve the same scoped
instances. Connections come from configuration (including Key Vault), not EF6
`App.config` connection-string lookup. `WorkbenchDbConfiguration` explicitly registers
the EF6 `System.Data.SqlClient` factory and provider services. This legacy provider is
required by the selected EF6 version and is deprecated; it is not replaced with the
incompatible EF Core SQL Server provider.

Workbench retains nine `IW` tables, SQL `datetime` columns, client-assigned GUID keys,
the `PackRequested.Id` and `SchemeNameDropId` identities, four required cascading
relationships and the optional non-cascading scheme relationship. People remains
externally owned: `OLA.People`, `OLA.Locations` and `OLA.Managers`, including existing
column names, lengths and the composite `(personId, managerId)` key.

Database initializers are disabled for **both** production contexts. Startup never
creates, drops or migrates either database. `ApplyMigrationsOnStartup=true` still
fails startup. Historical `Migrations/` files/resources remain excluded from compilation
and unchanged for reference; they must not be replayed automatically.
EF Core baseline artifacts and the local `dotnet-ef` tool manifest have been removed.
Do not run the former EF Core baseline deployment commands or alter existing
`__MigrationHistory`/`__EFMigrationsHistory` tables as part of this application change.

### Required schema acceptance and rollout

1. Back up the existing database and obtain DBA/application-owner approval. Use a restored
   representative non-production copy first. Confirm EF6 schema changes through the
   February 2024 model have already been applied.
2. Compare the live Workbench and externally owned People schemas with the mappings:
   tables/columns, types/lengths/nullability, key order, identity columns, foreign-key
   columns and delete actions. Also review physical constraint/index names, defaults,
   triggers, computed columns, permissions, collation and existing data. The archived
   Workbench model is source evidence, **not a live schema export**; People has no equivalent
   archived snapshot. Future schema changes must account for existing physical
   constraint/index names.
3. Configure the opt-in read-only SQL tests below with accounts permitted to read data
   and schema metadata. Run them against both approved databases. They verify all mapped
   columns, primary keys, foreign keys/delete actions and representative navigation reads,
   but do not prove all data or write behavior.
4. On the non-production copy, complete approved write/read-back acceptance for holding/
   breed edits, scheme controls, inspector assignment, pack creation/expiry/download,
   cattle/breed spreadsheet output, bulk import/clear and BCMS requests. Validate actual
   SQL identity generation, SQL date rounding and relevant external pack processors.
   Offline provider tests cannot certify these SQL Server details.
5. Deploy with external connection/identity settings and startup migrations disabled.
   The legacy `System.Data.SqlClient` provider does **not** share modern SqlClient's
   encryption defaults: require `Encrypt=True;TrustServerCertificate=False` in approved
   vault connection strings and provision a trusted SQL Server certificate. Validate
   Integrated Security/network access for the process identity.
6. Finish HTTPS/Entra, file/template, SMTP and holiday-service acceptance. Rollback
   restores the previous application deployment; this version change performs no schema
   operations. Future schema changes require a separately reviewed deployment script.

## Build and tests

From this directory:

```powershell
dotnet build .\RPA.Inspections.Workbench\RPA.Inspections.Workbench.csproj
dotnet test .\RPA.Inspections.Workbench.Tests\RPA.Inspections.Workbench.Tests.csproj
dotnet test .\RPA.Inspections.Workbench.Tests\RPA.Inspections.Workbench.Tests.csproj --filter "FullyQualifiedName~Persistence"
# Opt in by supplying WORKBENCH_SQL_TEST_CONNECTION and PEOPLE_SQL_TEST_CONNECTION externally:
dotnet test .\RPA.Inspections.Workbench.Tests\RPA.Inspections.Workbench.Tests.csproj --filter "TestCategory=SqlServerAcceptance"
```

The authentication tests use local signing keys/static OIDC metadata and an offline test
host; they do not require Entra, IDT, SQL, SMTP, or the holiday service. A real tenant sign-in
and representative staff-number/permission acceptance check are still required before go-live.

Tests cover EF6 mapping/snapshot parity, scoped contexts, startup migration rejection,
tracked/detached edits, lazy/eager loading, People audit lookups and BCMS/pack requests.
Behavioral persistence tests use isolated transient Effort.EF6 2.2.17 databases
(NMemory 3.1.4) with EF6 6.4.4, not EF Core or SQLite. Production database initializers
remain disabled; only test contexts create transient test databases.

## Container

The multi-stage [Dockerfile](./Dockerfile) follows the Defra .NET container
standard ([defra-docker-dotnetcore](https://github.com/DEFRA/defra-docker-dotnetcore)):
a `development` stage (`defradigital/dotnetcore-development`) restores, tests
and publishes the app; a `production` stage (`defradigital/dotnetcore`) runs
it as the non-root `dotnet` user with write permissions removed.
`DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` keeps ICU enabled for `en-GB`,
and it listens on HTTP 8080.

```powershell
docker build --target production -t inspections-workbench:local .
# Optional non-secret package source, replacing configured restore sources:
docker build --target production --build-arg NUGET_SOURCE="https://your-feed/v3/index.json" -t inspections-workbench:local .
```
Offline provider results are not a claim of SQL Server execution compatibility.
The SQL Server acceptance tests are opt-in and read-only; without explicit test
connections they are skipped. Live SQL schema/write parity and deployed tenant
acceptance remain **unverified**.

Verification on 2026-11-09: application build succeeded and the full Debug suite
passed **251 tests, 3 skipped, 0 failed** (254 total). The skips are the two
unconfigured live SQL acceptance tests and the existing redacted-recipient mail test.
Resolved application and test assets contain `EntityFramework/6.4.4` and no
`Microsoft.EntityFrameworkCore` packages. CPH was not changed.

Restore/build currently report **NU1904** for the pre-existing EPPlus transitive dependency
`System.Drawing.Common 4.7.0` (GHSA-rxg9-xrhp-64gj). It was not introduced or remediated by
this EF migration and needs a separately tested dependency/security follow-up.
