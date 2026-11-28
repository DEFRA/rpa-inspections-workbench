using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace RPA.Inspections.Workbench.Services;

public static class KeyVaultConfiguration
{
    public static ConfigurationManager AddWorkbenchKeyVault(this ConfigurationManager configuration,
        IHostEnvironment environment, SecretClient client = null)
    {
        var vaultUri = configuration["KeyVault:VaultUri"];
        var clientId = configuration["KeyVault:ManagedIdentityClientId"];
        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            if (!string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("KeyVault:ManagedIdentityClientId requires KeyVault:VaultUri.");
            return configuration;
        }

        if (!Uri.TryCreate(vaultUri, UriKind.Absolute, out var vault) ||
            vault.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(vault.Host) ||
            !vault.IsDefaultPort || vault.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(vault.UserInfo) || !string.IsNullOrEmpty(vault.Query) ||
            !string.IsNullOrEmpty(vault.Fragment))
            throw new InvalidOperationException("KeyVault:VaultUri must be the HTTPS vault endpoint without credentials, a path, query, fragment or custom port.");

        if (!string.IsNullOrWhiteSpace(clientId) &&
            (!Guid.TryParse(clientId, out var identityId) || identityId == Guid.Empty))
            throw new InvalidOperationException("KeyVault:ManagedIdentityClientId must be a user-assigned managed identity client ID.");

        if (client == null)
        {
            TokenCredential credential = environment.IsDevelopment()
                ? new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = string.IsNullOrWhiteSpace(clientId) ? null : clientId
                })
                : string.IsNullOrWhiteSpace(clientId)
                    ? new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned)
                    : new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId));
            client = new SecretClient(vault, credential);
        }

        configuration.AddAzureKeyVault(client, new Azure.Extensions.AspNetCore.Configuration.Secrets.AzureKeyVaultConfigurationOptions());

        // Check the vault provider itself so missing secrets cannot fall back to another source.
        var provider = ((IConfigurationRoot)configuration).Providers.Last();
        foreach (var name in new[] { "WorkbenchContext", "PeopleContext" })
        {
            if (!provider.TryGet($"ConnectionStrings:{name}", out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Key Vault must contain a nonempty 'ConnectionStrings--{name}' secret.");
        }

        return configuration;
    }
}
