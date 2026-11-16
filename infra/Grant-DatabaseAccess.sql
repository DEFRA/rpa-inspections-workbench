-- Run once per environment, per database (RPA_Inspections_Workbench AND the People database), by the DBA,
-- connected as the Entra SQL administrator (not as the runtime identity).
-- Replace the placeholder with the exact name of the user-assigned managed identity resource (the 'identity'
-- output of the foundation deployment, for example '<prefix>-<env>-id').
--
-- IMPORTANT (see infra/README.md "Known architecture gap"): granting this access does not by itself make the
-- application able to connect. Workbench's EF6 is pinned to EntityFramework 6.4.4 + System.Data.SqlClient 4.9.0,
-- which does not support "Authentication=Active Directory Managed Identity" in a connection string. This grant
-- is still the correct DB-side preparation for whichever resolution is chosen (EF6/SqlClient upgrade, or a
-- programmatic AccessToken change in WorkbenchContext/PeopleContext) -- it is run ahead of that decision, not
-- a sign that the connection already works end to end.
--
-- The runtime identity gets data access only: no DDL, no migration-history changes. The schema is released
-- separately by the DBA or a pipeline; no startup migration or Bicep SQL script creates schema/users.

DECLARE @identity sysname = N'<managed-identity-name>';

IF @identity = N'<managed-identity-name>'
    THROW 50000, 'Replace the placeholder with the managed identity name before running this script.', 1;

DECLARE @sql nvarchar(max);

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @identity)
BEGIN
    SET @sql = N'CREATE USER ' + QUOTENAME(@identity) + N' FROM EXTERNAL PROVIDER;';
    EXEC (@sql);
END

SET @sql = N'ALTER ROLE db_datareader ADD MEMBER ' + QUOTENAME(@identity) + N';';
EXEC (@sql);

SET @sql = N'ALTER ROLE db_datawriter ADD MEMBER ' + QUOTENAME(@identity) + N';';
EXEC (@sql);

-- The application reads and writes data only. Do not add db_owner or db_ddladmin.
-- Readiness checks (SELECT 1) need no extra permission.

-- Verify: the identity is a member of exactly the two data roles.
SELECT r.name AS role_name
FROM sys.database_role_members m
JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
WHERE u.name = @identity
ORDER BY r.name;
