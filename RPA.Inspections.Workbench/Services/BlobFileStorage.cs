using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RPA.Inspections.Workbench.Services;

public sealed class BlobFileStorage(BlobContainerClient container, ILogger<BlobFileStorage> logger) : IFileStorage
{
    public void Upload(string blobName, Stream content, string contentType)
    {
        ValidateName(blobName);
        try
        {
            container.GetBlobClient(blobName).Upload(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            });
        }
        catch (RequestFailedException exception)
        {
            LogFailure("upload", exception.Status);
            throw;
        }
    }

    public Stream Download(string blobName)
    {
        ValidateName(blobName);
        try
        {
            return container.GetBlobClient(blobName)
                .DownloadStreaming(new BlobDownloadOptions(), CancellationToken.None).Value.Content;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            logger.LogWarning("Blob download requested a file that does not exist.");
            throw new FileNotFoundException("The requested file was not found in Blob Storage.", exception);
        }
        catch (RequestFailedException exception)
        {
            LogFailure("download", exception.Status);
            throw;
        }
    }

    public void Delete(string blobName)
    {
        ValidateName(blobName);
        try
        {
            if (!container.GetBlobClient(blobName).DeleteIfExists().Value)
                logger.LogInformation("Blob deletion was already complete; the file does not exist.");
        }
        catch (RequestFailedException exception)
        {
            LogFailure("delete", exception.Status);
            throw;
        }
    }

    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 1024 ||
            (!name.StartsWith("packs/", StringComparison.Ordinal) &&
             !name.StartsWith("reports/", StringComparison.Ordinal)) ||
            name.Any(character => char.IsControl(character) || character is '\\' or ':') ||
            name.Split('/').Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            logger.LogWarning("Rejected an invalid Blob Storage file reference.");
            throw new InvalidOperationException("The file reference must be a relative packs/ or reports/ blob name. Migrate legacy local file paths before use.");
        }
    }

    private void LogFailure(string operation, int status) =>
        logger.LogError("Blob Storage {Operation} failed. Status: {Status}.", operation, status);
}
