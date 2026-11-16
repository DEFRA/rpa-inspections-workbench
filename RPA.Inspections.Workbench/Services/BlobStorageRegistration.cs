using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;

namespace RPA.Inspections.Workbench.Services;

public static class BlobStorageRegistration
{
    public static IServiceCollection AddWorkbenchBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["BlobStorage:ServiceUri"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var serviceUri) ||
            serviceUri.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(serviceUri.Host) ||
            !serviceUri.IsDefaultPort || serviceUri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(serviceUri.UserInfo) || !string.IsNullOrEmpty(serviceUri.Query) ||
            !string.IsNullOrEmpty(serviceUri.Fragment))
            throw new InvalidOperationException("BlobStorage:ServiceUri must be the HTTPS Blob service endpoint without credentials, a path, query, fragment or custom port.");

        var containerName = configuration["BlobStorage:ContainerName"];
        if (containerName == null || !Regex.IsMatch(containerName, @"\A[a-z0-9][a-z0-9-]{1,61}[a-z0-9]\z") ||
            containerName.Contains("--"))
            throw new InvalidOperationException("BlobStorage:ContainerName must be a valid lowercase Azure container name (3-63 characters).");

        var clientId = configuration["BlobStorage:ManagedIdentityClientId"];
        if (!string.IsNullOrWhiteSpace(clientId) &&
            (!Guid.TryParse(clientId, out var identityId) || identityId == Guid.Empty))
            throw new InvalidOperationException("BlobStorage:ManagedIdentityClientId must be a user-assigned managed identity client ID.");

        services.AddSingleton(provider =>
        {
            TokenCredential credential = provider.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = string.IsNullOrWhiteSpace(clientId) ? null : clientId
                })
                : new ManagedIdentityCredential(string.IsNullOrWhiteSpace(clientId)
                    ? ManagedIdentityId.SystemAssigned
                    : ManagedIdentityId.FromUserAssignedClientId(clientId));
            return new BlobServiceClient(serviceUri, credential).GetBlobContainerClient(containerName);
        });
        services.AddSingleton<IFileStorage, BlobFileStorage>();
        return services;
    }
}
