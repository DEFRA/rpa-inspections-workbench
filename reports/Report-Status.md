# Migration Status — RPA Inspections Workbench
**Updated:** 2026-11-17T00.31Z

| Phase | Status | Notes |
|---|---|---|
| Assess (Discovery) | Done (prior session) | See eports/rpa-inspections-workbench.md (EF6 data-access assessment) |
| Phase 1 - Plan | Not started | |
| Phase 2 - Code migration | Partially done (this session) | Cloud-standard code alignment only: forwarded headers, health endpoints, Data Protection, Application Insights |
| Phase 3 - Infrastructure generation | Done (this session) | infra/main.bicep + infra/app.bicep created and compiled; application not yet deployed |
| Phase 4 - Deployment | Not started | Blocked on PENDING items below |
| Phase 5 - CI/CD | Not started | D-16: keep existing CI/CD as-is; no pipeline authored |
| Phase 6 - Post-migration ops | Not started | |

## Completed this session

- **Resolved the SQL connectivity architecture gap** (see below) by upgrading Workbench's EF6 SQL provider to
  `Microsoft.Data.SqlClient`, mirroring `rpa-mts-inspections`'s already-working pattern exactly:
  - `RPA.Inspections.Workbench.csproj`: bumped `EntityFramework` 6.4.4 → 6.5.2, removed
    `System.Data.SqlClient 4.9.0`, added `Microsoft.EntityFramework.SqlServer 6.5.2` (confirmed via
    `project.assets.json` that this transitively pulls in `Microsoft.Data.SqlClient 6.1.4`, identical to
    `rpa-mts-inspections`'s resolved package set).
  - `DAL\WorkbenchDbConfiguration.cs`: rewritten to the `MicrosoftSqlProviderServices`-based pattern (provider
    factory/services, migration SQL generator, `MicrosoftSqlAzureExecutionStrategy` for Azure SQL transient-fault
    retry), still shared by both `WorkbenchContext` and `PeopleContext`.
  - `DAL\WorkbenchContext.cs`, `DAL\PeopleContext.cs`, `Services\ApplicationServices.cs`,
    `Services\IdtRoleClaimsTransformation.cs`: `using System.Data.SqlClient;` → `using Microsoft.Data.SqlClient;`.
  - `Services\ApplicationServices.cs`: `ValidateSqlConnection` now resolves `Microsoft.Data.SqlClient.SqlConnectionStringBuilder`,
    which supports `Authentication=Active Directory Managed Identity` (the prior `System.Data.SqlClient` builder did
    not); error message text updated to drop the now-inaccurate "System.Data.SqlClient" wording.
  - `Migrations\Configuration.cs`: hardcoded `"System.Data.SqlClient"` provider-invariant-name literal replaced with
    `MicrosoftSqlProviderServices.ProviderInvariantName`.
  - `RPA.Inspections.Workbench.Tests.csproj`: `EntityFramework` pin `[6.4.4]` → `[6.5.2]` (required so the test
    project's exact version pin doesn't conflict with the project reference's upgraded version).
  - `Persistence\Ef6TestModel.cs`, `Persistence\MappingProviderTests.cs`: same using-statement switch; `Ef6TestModel`'s
    `DbProviderInfo("System.Data.SqlClient", ...)` literal replaced with `MicrosoftSqlProviderServices.ProviderInvariantName`.
  - **Added** `Persistence\SqlServerProviderTests.cs`: new LocalDB-backed regression test proving the
    `Microsoft.Data.SqlClient` provider path works end-to-end (real connection, schema creation from the current EF6
    model, CRUD round-trip through `WorkbenchContext`). Skips gracefully via `Assert.Ignore` when LocalDB is
    unavailable (as in this sandbox). Deliberately does not replay Workbench's historical, deployment-managed
    migrations (excluded from compilation by design — see `ArchivedMigrationsAndEfCoreBaselineAreNotExecutable`).
  - Build: `dotnet build` succeeds, 0 errors, on both the main and test projects. Tests:
    279 passed / 9 pre-existing unrelated failures / 6 skipped (3 pre-existing + 3 new `SqlServerProviderTests`,
    skipped because LocalDB is not installed in this sandbox) — no regressions against the 279/9/3 baseline.
- Created `infra/` (Bicep + Azure Verified Modules) for Azure Container Apps hosting, mirroring
  `rpa-cph-non-subsidy\infra\` and `rpa-mts-inspections\infra\` (closest sibling, same "Inspections" domain family).
- Carried over portfolio-wide decisions already `✅ DECIDED` in `rpa-claim-statements\reports\Decisions-Required.md`
  (same DEFRA RPA portfolio, not re-decided per application):
  - **D-04** Database engine = Azure SQL Database
  - **D-06** Hosting platform = Azure Container Apps
  - **D-09** Authentication = Entra ID via Microsoft.Identity.Web
  - **D-11** Compliance scope = GDPR / UK-GDPR applicability confirmed
  - **D-16** CI/CD platform = keep existing CI/CD as-is (no pipeline authored in this pass)
  - **D-17** Observability stack = Application Insights
  - **D-18** Container registry = Azure Container Registry
  - **D-07** (IaC tool) remains formally `⏸ PENDING` at the portfolio level; Bicep is used here only because it is
    the de-facto standard already applied consistently across every sibling project with built infra.
- Aligned `RPA.Inspections.Workbench` application code with the established cloud-standard pattern:
  - `Program.cs`: added `app.UseForwardedHeaders()`, `/health/live` and `/health/ready` endpoint mappings
    (`AllowAnonymous`), and an HTTPS-redirect exemption for those two paths.
  - Added `Services\TrustedForwarding.cs` (`AddWorkbenchForwarding`), mirroring `rpa-cph-non-subsidy`'s
    `AddCphForwarding` — reads `ReverseProxy:KnownProxies` / `ReverseProxy:KnownIPNetworks`, fails safe to
    `ForwardedHeaders.None` when unconfigured.
  - Rewrote `Services\ApplicationInsightsExtensions.cs` (`AddWorkbenchApplicationInsights`): was a no-op stub
    (commented-out SDK call, no package reference); now registers `Microsoft.ApplicationInsights.AspNetCore` with
    optional managed-identity-authenticated ingestion, mirroring `rpa-cph-non-subsidy`'s
    `AddCphApplicationInsights`.
  - Rewrote `Services\DataProtectionExtensions.cs` (`AddWorkbenchDataProtection`): was local-filesystem-only
    (no Blob/Key Vault wrapping, would not survive a Container Apps replica/restart); now persists keys to Azure
    Blob Storage and wraps them with a Key Vault key via a user-assigned managed identity when
    `DataProtection:Enabled` is true, falling back to the previous local behavior otherwise. Mirrors
    `rpa-cph-non-subsidy`'s `SharedDataProtection` / `rpa-mts-inspections`'s `SharedDataProtection`.
  - `Services\ApplicationServices.cs`: wired `AddWorkbenchForwarding` into the DI pipeline.
  - `RPA.Inspections.Workbench.csproj`: added `Microsoft.ApplicationInsights.AspNetCore` 2.23.0,
    `Azure.Extensions.AspNetCore.DataProtection.Blobs` 1.5.5, `Azure.Extensions.AspNetCore.DataProtection.Keys` 1.6.5.
  - `appsettings.json`: removed the two on-prem `SQLEXPRESS`/`Trusted_Connection` connection strings
    (`PeopleConnection`, `DefaultConnection`) — dev artifacts that are superseded by the Key-Vault-sourced
    `ConnectionStrings--WorkbenchContext` / `ConnectionStrings--PeopleContext` secrets the app already requires
    at runtime (`Services\KeyVaultConfiguration.cs`). This also fixed a previously-failing test,
    `ShippedSettingsDoNotContainSqlConnectionStrings("appsettings.json")`.
- Build: `dotnet build` succeeds, 0 errors. Tests: 279 passed / 9 pre-existing unrelated failures (down from 10 at
  baseline — the appsettings.json fix above resolved one) / 3 skipped, no regressions introduced.
- Validated both Bicep templates with `az bicep build` (0 errors, 0 warnings) and ran `Test-Templates.ps1` /
  `Test-KeyBootstrap.ps1` end to end under PowerShell 7 (installed for this validation pass) — all scenarios passed.

## SQL connectivity gap — RESOLVED this session

Workbench's EF6 was pinned to `EntityFramework 6.4.4` + `System.Data.SqlClient 4.9.0`
(`DAL\WorkbenchDbConfiguration.cs` explicitly registered `SqlClientFactory`), which did **not** support
`Authentication=Active Directory Managed Identity` connection strings — unlike `rpa-mts-inspections`
(`EntityFramework 6.5.2` + `Microsoft.Data.SqlClient`). This session upgraded Workbench's EF6 to `6.5.2` +
`Microsoft.EntityFramework.SqlServer 6.5.2`, mirroring `rpa-mts-inspections`'s already-working pattern exactly,
switching the ADO.NET provider to `Microsoft.Data.SqlClient` across `DAL\`, `Services\ApplicationServices.cs`, and
`Migrations\Configuration.cs`. `main.bicep` still provisions Azure SQL **AAD-only** to match D-04 — unchanged by
this fix — and Managed Identity connection strings are now a supported, working option against it. See
`infra/README.md` "SQL connectivity" for the resolved connection-string format. This was purely an EF6 provider
upgrade; no EF6/SQL access pattern or AAD-only requirement was changed.

## Risks and blockers

- All items in "Explicitly PENDING" below block `az deployment group create` for both `main.bicep` and
  `app.bicep` (no default is set for any of them).

## Files created or updated

- `+infra/main.bicep`, `+infra/app.bicep`, `+infra/modules/private-endpoint.bicep`
- `+infra/main.parameters.example.json`, `+infra/app.parameters.example.json`
- `+infra/README.md`, `+infra/Initialize-DataProtectionKey.ps1`, `+infra/Test-KeyBootstrap.ps1`,
  `+infra/Test-Templates.ps1`, `+infra/Grant-DatabaseAccess.sql`,
  `+infra/entra/New-WorkbenchEntraRegistration.ps1`
- `~RPA.Inspections.Workbench/Program.cs`
- `+RPA.Inspections.Workbench/Services/TrustedForwarding.cs`
- `~RPA.Inspections.Workbench/Services/ApplicationInsightsExtensions.cs`
- `~RPA.Inspections.Workbench/Services/DataProtectionExtensions.cs`
- `~RPA.Inspections.Workbench/Services/ApplicationServices.cs`
- `~RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj`
- `~RPA.Inspections.Workbench/appsettings.json`
- `~RPA.Inspections.Workbench/DAL/WorkbenchDbConfiguration.cs`
- `~RPA.Inspections.Workbench/DAL/WorkbenchContext.cs`
- `~RPA.Inspections.Workbench/DAL/PeopleContext.cs`
- `~RPA.Inspections.Workbench/Services/IdtRoleClaimsTransformation.cs`
- `~RPA.Inspections.Workbench/Migrations/Configuration.cs`
- `~RPA.Inspections.Workbench.Tests/RPA.Inspections.Workbench.Tests.csproj`
- `~RPA.Inspections.Workbench.Tests/Persistence/Ef6TestModel.cs`
- `~RPA.Inspections.Workbench.Tests/Persistence/MappingProviderTests.cs`
- `+RPA.Inspections.Workbench.Tests/Persistence/SqlServerProviderTests.cs`
- `~infra/README.md`, `~infra/app.bicep`
- `+reports/Report-Status.md` (this file)

## Recommended next command

- Resolve the explicitly PENDING items below with the user, then /Phase4-DeployToAzure once infra parameters are
  settled and the two connection-string Key Vault secrets are bootstrapped.

## Explicitly PENDING — needs the user's sign-off

- **D-07** (IaC tool) — formally still PENDING at the portfolio level; Bicep used here as the de-facto standard only.
- **Region / `location`** — no default; must be explicitly approved.
- **SQL administrator identity** (`sqlAdminName` / `sqlAdminObjectId` / `sqlAdminPrincipalType`).
- **Cost ceiling** (D-12 in the carried-over decision log) — not estimated here.
- **DR / RPO-RTO targets** (D-13) — zone/backup redundancy parameterized but not pre-selected.
- **SMTP relay target** for `Mail:Host`/`Mail:Port` — the on-prem UNC pickup directory cannot work on Linux.
- **`bank-holidays` service URL** once that sibling app is itself hosted on Azure.
- **Container registry image publishing / CI/CD** — out of scope; D-16 keeps existing CI/CD unchanged.

## 📜 Action Log

<!--
Every prompt, hook, and agent appends one line per meaningful action.
Format: - <ISO-8601-UTC> | actor=<name> | action=<verb-phrase> | files=<+~-> | tokens=~<bucket> | turn=<n> | notes="<free text>"
See .github/skills/action-log-format.md for the full spec.
-->

- 2026-11-16T17.59Z | actor=User | action=session-started | tokens=~0 | turn=0 | notes="requested infra + cloud-standard alignment for rpa-inspections-workbench"
- 2026-11-16T17.59Z | actor=Code-Migration-Modernization | action=produced-infra-plan | files=+infra/main.bicep,+infra/app.bicep,+infra/modules/private-endpoint.bicep,+infra/main.parameters.example.json,+infra/app.parameters.example.json,+infra/README.md,+infra/Initialize-DataProtectionKey.ps1,+infra/Test-KeyBootstrap.ps1,+infra/Test-Templates.ps1,+infra/Grant-DatabaseAccess.sql,+infra/entra/New-WorkbenchEntraRegistration.ps1 | tokens=~30k | turn=1 | notes="mirrored rpa-cph-non-subsidy/rpa-mts-inspections; carried over D-04/D-06/D-09/D-11/D-16/D-17/D-18; D-07 de-facto only"
- 2026-11-16T17.59Z | actor=Code-Migration-Modernization | action=code-modernized | files=~RPA.Inspections.Workbench/Program.cs,+RPA.Inspections.Workbench/Services/TrustedForwarding.cs,~RPA.Inspections.Workbench/Services/ApplicationInsightsExtensions.cs,~RPA.Inspections.Workbench/Services/DataProtectionExtensions.cs,~RPA.Inspections.Workbench/Services/ApplicationServices.cs,~RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj,~RPA.Inspections.Workbench/appsettings.json | tokens=~12k | turn=1 | notes="forwarded headers, health endpoints, Key-Vault-wrapped Data Protection, Application Insights SDK; removed legacy SQLEXPRESS connection strings"
- 2026-11-16T17.59Z | actor=Code-Migration-Modernization | action=infra-validated | tokens=~2k | turn=1 | notes="az bicep build 0 errors/0 warnings on main.bicep + app.bicep; Test-Templates.ps1 + Test-KeyBootstrap.ps1 passed under PowerShell 7; dotnet build 0 errors; dotnet test 279 passed / 9 pre-existing unrelated failures / 3 skipped, no regressions"
- 2026-11-16T17.59Z | actor=Code-Migration-Modernization | action=phase-blocked | reason="SQL connectivity: System.Data.SqlClient does not support AAD Managed Identity; region/SQL-admin/cost/DR/SMTP/bank-holidays-URL all PENDING" | tokens=~500 | turn=1
- 2026-11-17T00.31Z | actor=Code-Migration-Modernization | action=sql-connectivity-gap-resolved | files=~RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj,~RPA.Inspections.Workbench/DAL/WorkbenchDbConfiguration.cs,~RPA.Inspections.Workbench/DAL/WorkbenchContext.cs,~RPA.Inspections.Workbench/DAL/PeopleContext.cs,~RPA.Inspections.Workbench/Services/ApplicationServices.cs,~RPA.Inspections.Workbench/Services/IdtRoleClaimsTransformation.cs,~RPA.Inspections.Workbench/Migrations/Configuration.cs,~RPA.Inspections.Workbench.Tests/RPA.Inspections.Workbench.Tests.csproj,~RPA.Inspections.Workbench.Tests/Persistence/Ef6TestModel.cs,~RPA.Inspections.Workbench.Tests/Persistence/MappingProviderTests.cs,+RPA.Inspections.Workbench.Tests/Persistence/SqlServerProviderTests.cs,~infra/README.md,~infra/app.bicep | tokens=~18k | turn=2 | notes="upgraded EF6 to 6.5.2 + Microsoft.EntityFramework.SqlServer 6.5.2, mirroring rpa-mts-inspections exactly; provider now Microsoft.Data.SqlClient, so Authentication=Active Directory Managed Identity connection strings are supported; AAD-only SQL auth and hosting model unchanged; dotnet build 0 errors; dotnet test 279 passed / 9 pre-existing unrelated failures / 6 skipped (3 pre-existing + 3 new LocalDB-gated tests, skipped here because LocalDB is not installed), no regressions"
