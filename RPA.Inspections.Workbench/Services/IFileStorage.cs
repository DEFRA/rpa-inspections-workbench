namespace RPA.Inspections.Workbench.Services;

public interface IFileStorage
{
    void Upload(string blobName, Stream content, string contentType);
    Stream Download(string blobName);
    void Delete(string blobName);
}
