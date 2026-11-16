# Bicep handoff — RPA Inspections Workbench

This is a repository-only infrastructure handoff, not a deployment or production
approval. The receiving team supplies approved parameter values and manually
tests in its own subscription. No regional, quota, Azure Policy, what-if, cost,
recovery, compliance or deployed-runtime checks have been performed here.

This infra mirrors the already-built `rpa-cph-non-subsidy\infra\` and
`rpa-mts-inspections\infra\` (same "Inspections" domain family) so that the three
apps share one operational pattern. It also carries over the architecture
decisions already recorded as `✅ DECIDED` in the sibling
`rpa-claim-statements\reports\Decisions-Required.md` / `Decision-Log.md`, since
those are portfolio-wide (DEFRA RPA), not per-application:

| ID | Decision | Carried-over value |
|---|---|---|
| D-04 | Database engine | Azure SQL Database |
| D-06 | Hosting platform | Azure Container Apps |
| D-09 | Authentication | Entra ID via Microsoft.Identity.Web |
| D-11 | Compliance scope | GDPR / UK-GDPR applicability confirmed (not "None") |
| D-16 | CI/CD platform | Keep existing CI/CD as-is; no pipeline authored here |
| D-17 | Observability stack | Application Insights |
| D-18 | Container registry | Azure Container Registry |

**D-07 (IaC tool) is still formally `⏸ PENDING` at the portfolio level** — it was
explicitly deferred, not decided. Bicep + Azure Verified Modules is used here
only because it is already the de-facto, consistently-applied standard across
every sibling project that has built infra (`rpa-cph-non-subsidy`,
`rpa-mts-inspections`, `rpa-quality-checks`, `people-api`), not because D-07 has
been formally closed. If the portfolio later formally decides a different IaC
tool, this folder would need to be regenerated.

## Files and stages

- [main.bicep](./main.bicep): foundation — network/private DNS/endpoints,
  user-assigned identity, ACR, Key Vault, private key-ring + application-files
  Blob storage, Azure SQL (**two databases**: Workbench and People, on one
  logical server), monitoring and ACA environment. **No application starts.**
- [app.bicep](./app.bicep): application activation after the receiving-team
  prerequisites below. Uses the same foundation names and identity, and grants
  the runtime identity crypto access on the already bootstrapped wrapping key.
- [modules/private-endpoint.bicep](./modules/private-endpoint.bicep): generic
  private endpoint + private DNS zone helper, unchanged from the sibling apps.
- [Initialize-DataProtectionKey.ps1](./Initialize-DataProtectionKey.ps1):
  separate operator-run, private-network key creation and rotation-policy setup.
  Unchanged from the sibling apps (fully generic, no app-specific strings).
- [Test-Templates.ps1](./Test-Templates.ps1): local compilation, synthetic
  negative tests (adapted for Workbench's two databases / four roles / mail /
  holiday-service parameters) and optional parameter-file validation. Requires
  **PowerShell 7+** (`ConvertFrom-Json -AsHashtable`); it does not log in, query
  Azure or deploy.
- [Test-KeyBootstrap.ps1](./Test-KeyBootstrap.ps1): offline unit tests for
  `Initialize-DataProtectionKey.ps1`. Unchanged from the sibling apps.
- [Grant-DatabaseAccess.sql](./Grant-DatabaseAccess.sql): DBA-run grant script,
  run once per database (both Workbench and People). See "Known architecture
  gap" below before assuming this alone makes the connection work.
- [entra/New-WorkbenchEntraRegistration.ps1](./entra/New-WorkbenchEntraRegistration.ps1):
  identity-team-run Entra app registration helper for Workbench's four app
  roles (BasicAccess, Admin, SuperUser, Bcms). Adapted from
  `rpa-mts-inspections\infra\entra\New-MtsEntraRegistration.ps1`.
- Parameter examples are deliberately incomplete: `null` means **unknown**,
  not a valid production default. Copy them outside Git and supply every value.
  Their checked-in state is intentionally not deployable.

Both entry points target an existing resource group. The receiving team owns
resource-group creation, subscription selection and permissions. Supply the
same resource group, location, prefix, environment and tags for both stages.

## Local checks

Use an installed [Bicep CLI](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install)
and **PowerShell 7+** (`pwsh`).

```powershell
pwsh ./infra/Test-Templates.ps1 -BicepPath 'C:\tools\bicep.exe'
pwsh ./infra/Test-Templates.ps1 -BicepPath 'C:\tools\bicep.exe' `
  -FoundationParameters 'C:\private\workbench\foundation.parameters.json' `
  -ApplicationParameters 'C:\private\workbench\application.parameters.json'
```

Both `main.bicep` and `app.bicep` were validated in this pass with
`az bicep build --file <path> --outdir <temp>` (Azure CLI 2.93.0, bundled Bicep
CLI) — **0 errors, 0 warnings** — and `Test-Templates.ps1` / `Test-KeyBootstrap.ps1`
were run end to end under PowerShell 7.7 (installed for this validation pass) —
all synthetic scenarios and 27 offline key-bootstrap tests passed. No Azure
deployment, what-if, quota, cost or RBAC-propagation check was performed.

## SQL connectivity (architecture gap resolved this session)

Workbench's EF6 was pinned to `EntityFramework 6.4.4` + `System.Data.SqlClient
4.9.0`, whose ADO.NET provider does **not** support `Authentication=Active
Directory Managed Identity` connection strings. This session upgraded
Workbench's EF6 to `6.5.2` + `Microsoft.EntityFramework.SqlServer 6.5.2`
(mirroring `rpa-mts-inspections`'s already-working pattern exactly), which
switches the ADO.NET provider to `Microsoft.Data.SqlClient` throughout
`DAL\WorkbenchDbConfiguration.cs`, `DAL\WorkbenchContext.cs`,
`DAL\PeopleContext.cs`, `Services\ApplicationServices.cs`, and
`Migrations\Configuration.cs`. `main.bicep` continues to provision Azure SQL
**AAD-only** (`azureADOnlyAuthentication: true`) to match the portfolio D-04
standard — unchanged by this fix — and Managed Identity connection strings are
now a supported option against it. No EF6/SQL access pattern was changed
beyond the provider swap; the AAD-only requirement was not relaxed.

One item remains unchanged and still requires owner action (not an
architecture blocker, just a one-time manual step):

- **Connection strings are Key Vault secrets, not container app env vars.**
  Workbench's existing (untouched) `Services\KeyVaultConfiguration.cs` requires
  `ConnectionStrings--WorkbenchContext` and `ConnectionStrings--PeopleContext`
  to exist as literal secrets in the foundation Key Vault — verified against
  `RPA.Inspections.Workbench.Tests\Configuration\KeyVaultConfigurationTests.cs`.
  This differs from `rpa-mts-inspections`/`rpa-cph-non-subsidy`, which set their
  single connection string directly as a container app environment variable.
  `app.bicep` therefore does **not** set these two settings; the owner bootstraps
  them directly in Key Vault (see "Owner bootstrap" step 4 below), the same way
  `entraClientSecretName` is already bootstrapped out of band.

Now that the provider supports it, the owner should populate those two secrets
with the Managed Identity connection-string format, using the `sqlServerHost`,
`sqlDatabaseName`/`peopleDatabaseName`, and `managedIdentityClientId` outputs
already exposed by `main.bicep`/`app.bicep`:

```text
Server=tcp:<sqlServerHost>,1433;Database=<databaseName>;Authentication=Active Directory Managed Identity;User Id=<managedIdentityClientId>;Encrypt=True;TrustServerCertificate=False;
```

`Grant-DatabaseAccess.sql` prepares the DB-side grant for the managed identity
against **both** the Workbench and People databases; it still needs to be run
by the DBA as part of the owner bootstrap below.

## Required foundation inputs

Every input is required; none selects a region or production sizing for you.
Unchanged from the sibling apps except `databaseName` is now two inputs.

| Inputs | Contract |
|---|---|
| `location`, `tags` | Owner-approved region and tag object; no default region. **PENDING** — no region has been approved for this application; do not infer one from the sibling apps. |
| `prefix`, `environmentName` | Lowercase alphanumeric, 2-6 and 2-3 characters. |
| `vnetCidr`, `acaSubnetCidr`, `privateEndpointSubnetCidr` | Approved, canonical RFC1918 IPv4 ranges; nonoverlapping subnets inside VNet. ACA subnet /27 or larger. |
| `acaZoneRedundant` | Explicit zone choice, subject to Consumption/region support. |
| `acrSku`, `acrZoneRedundant` | Basic/Standard/Premium; zone redundancy requires Premium. |
| `logRetentionDays`, `applicationInsightsRetentionDays` | Explicit supported retention. |
| `keyRingStorageSku` | Explicit Standard LRS/ZRS/GRS/GZRS choice. |
| `blobSoftDeleteDays`, `vaultSoftDeleteDays` | Blob/container soft-delete 1-365 days; vault soft-delete 7-90 days. |
| `wrappingKeySize`, `wrappingKeyRotationDays`, `wrappingKeyExpiryDays` | RSA 2048/3072/4096; rotation strictly before expiry. Applied via the private-network bootstrap step, not by ARM. |
| `sqlAdminName`, `sqlAdminObjectId`, `sqlAdminPrincipalType` | Supplied deployment-tenant Entra administrator. **PENDING** — no SQL administrator identity has been approved for this application. |
| `databaseName` | Explicit Workbench database name (letters/digits/underscore/hyphen). |
| `peopleDatabaseName` | Explicit People database name, on the **same** logical server, must differ from `databaseName`. |
| `sqlMinVcores`, `sqlMaxVcores`, `sqlMaxSizeGiB` | Explicit GP serverless Gen5 min/max/storage, applied to **both** databases identically; split them in `main.bicep` if the two databases need different sizing. |
| `sqlZoneRedundant`, `sqlBackupRedundancy` | Explicit zone and backup choices. Not an RPO/RTO. **PENDING** — see D-13 in the carried-over decision log. |
| `sqlPointInTimeRetentionDays`, `sqlLongTermRetention` | Explicit PITR and LTR policy. |

## Required application inputs

Copy [app.parameters.example.json](./app.parameters.example.json) outside Git
and replace every `null`.

| Inputs | Contract |
|---|---|
| `location`, `prefix`, `environmentName`, `tags`, `databaseName`, `peopleDatabaseName` | Match foundation exactly. |
| `imageRepository`, `imageDigest` | Repository inside the new ACR and immutable `sha256:` digest obtained after receiving-team push. |
| `entraTenantId`, `entraClientId` | Supplied production Entra tenant/application IDs. |
| `entraClientSecretName` | Existing owner-populated secret **name** in the new Key Vault, not a value. |
| `basicAccessRole`, `adminRole`, `superUserRole`, `bcmsRole` | Four distinct Entra app-role token values, matching `Authorization:BasicAccessRole` / `AdminRole` / `SuperUserRole` / `BcmsRole`. |
| `auditNameClaimType` | Exact unique staff-number audit claim contract (see `ApplicationServices.GetAuditName`). |
| `mailHost`, `mailPort` | SMTP relay. **PENDING** — no production SMTP relay has been approved; the on-prem `Mail:PickupDirectory` UNC path (`\\D1VMSMA001\inetpub\mailroot\Pickup`) is a Windows file share and cannot work inside a Linux container, so it is intentionally not wired here. |
| `holidayServiceWorkingDaysUrl` | **PENDING** — the on-prem URL (`http://d2vmbwa001/defra.bankholidays/api/Values/workingdays`) is not reachable from Azure. The sibling `bank-holidays` (DEFRA.BankHolidays) app in this repository is the intended eventual target once it is itself hosted on Azure; do not hardcode that assumption until it is actually deployed and its URL confirmed. |
| `containerSize` | Explicit supported CPU:memory pair. |
| `minReplicas`, `maxReplicas`, `concurrentRequests` | Explicit bounds/HTTP threshold. |
| `terminationGracePeriodSeconds` | Explicit shutdown allowance, 1-600 seconds. |

Shared Data Protection, trusted-proxy forwarding, HTTPS redirection and
Application Insights wiring are identical in shape to `rpa-mts-inspections`;
see `app.bicep`'s `env` block. `/health/live` and `/health/ready` are mapped in
`Program.cs` (added in this pass) and backed by the existing
`SqlReadinessHealthCheck` (tag `ready`), already registered in
`Services\ApplicationServices.cs`.

## Manual deployment checkpoints

These are **receiving-team commands**, not commands executed by this handoff.

### 1. Foundation

```powershell
az account set --subscription '<approved-subscription-id>'
az deployment group what-if --resource-group '<approved-resource-group>' `
  --template-file '.\infra\main.bicep' `
  --parameters '@C:\private\workbench\foundation.parameters.json'
az deployment group create --name 'workbench-foundation' `
  --resource-group '<approved-resource-group>' `
  --template-file '.\infra\main.bicep' `
  --parameters '@C:\private\workbench\foundation.parameters.json'
```

### 2. Owner bootstrap, from an approved private-network path

Same sequence as `rpa-cph-non-subsidy\infra\README.md` step 2 (private DNS
verification, image push, Entra registration via
`entra\New-WorkbenchEntraRegistration.ps1`, wrapping-key bootstrap via
`Initialize-DataProtectionKey.ps1`), plus two Workbench-specific steps:

4. Populate `ConnectionStrings--WorkbenchContext` and
   `ConnectionStrings--PeopleContext` directly as Key Vault secrets (not as
   `app.bicep` parameters — see "SQL connectivity" above), using the Managed
   Identity connection-string format shown there.
5. The DBA runs [Grant-DatabaseAccess.sql](./Grant-DatabaseAccess.sql) against
   **both** the Workbench and People databases for the output managed identity.

### 3. Application activation

```powershell
az deployment group what-if --resource-group '<approved-resource-group>' `
  --template-file '.\infra\app.bicep' `
  --parameters '@C:\private\workbench\application.parameters.json'
az deployment group create --name 'workbench-application' `
  --resource-group '<approved-resource-group>' `
  --template-file '.\infra\app.bicep' `
  --parameters '@C:\private\workbench\application.parameters.json'
```

The application will not reach a healthy `/health/ready` state until the two
connection-string secrets above are populated with a working value.

## Module pins and validation boundary

Identical AVM pins to `rpa-cph-non-subsidy`/`rpa-mts-inspections`: identity
0.6.0; VNet 0.10.2; Log Analytics 0.16.1; Application Insights 0.8.0; ACR
0.13.1; Key Vault 0.14.2; Storage 0.33.1; SQL 0.22.1; ACA environment 0.16.0;
ACA application 0.23.0. Compilation does not exercise ARM authorization,
private data-plane access, provider policies or RBAC propagation.

## Explicitly PENDING — needs the user's sign-off before deployment

- **D-07 (IaC tool)** — formally still PENDING at the portfolio level; Bicep is
  used here as the established de-facto standard, not a formal re-decision.
- **Region / `location`** — no default is set in `main.bicep`; must be supplied.
- **SQL administrator identity** (`sqlAdminName` / `sqlAdminObjectId` / `sqlAdminPrincipalType`).
- **Cost ceiling** (D-12 in the carried-over decision log) — not estimated here.
- **DR / RPO-RTO targets** (D-13) — `sqlZoneRedundant`/`sqlBackupRedundancy` are
  parameterized but not pre-selected for this reason.
- **SMTP relay target** for `mailHost`/`mailPort`.
- **`bank-holidays` service URL** once that sibling app is itself hosted on Azure.
- **Container registry image publishing / CI/CD** — out of scope for this infra
  generation pass; D-16 says keep whatever CI/CD already exists unchanged.
