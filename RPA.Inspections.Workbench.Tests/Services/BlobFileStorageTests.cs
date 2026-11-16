using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.Services;

namespace RPA.Inspections.Workbench.Tests.Services;

[TestFixture]
public class BlobFileStorageTests
{
    private Mock<BlobContainerClient> container;
    private Mock<BlobClient> blob;
    private BlobFileStorage storage;

    [SetUp]
    public void SetUp()
    {
        container = new Mock<BlobContainerClient>(MockBehavior.Strict);
        blob = new Mock<BlobClient>(MockBehavior.Strict);
        container.Setup(value => value.GetBlobClient(It.IsAny<string>())).Returns(blob.Object);
        storage = new BlobFileStorage(container.Object, NullLogger<BlobFileStorage>.Instance);
    }

    [Test]
    public void UploadPreservesBytesAndContentTypeAndCannotOverwriteExistingBlob()
    {
        byte[] uploaded = null;
        BlobUploadOptions uploadedOptions = null;
        blob.Setup(value => value.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Callback((Stream content, BlobUploadOptions options, CancellationToken cancellationToken) =>
            {
                using var captured = new MemoryStream();
                content.CopyTo(captured);
                uploaded = captured.ToArray();
                uploadedOptions = options;
            }).Returns(Mock.Of<Response<BlobContentInfo>>());
        using var source = new MemoryStream(new byte[] { 1, 2, 3 });
        storage.Upload("packs/request/inspection.xlsm", source, "application/vnd.ms-excel.sheet.macroEnabled.12");

        Assert.That(uploaded, Is.EqualTo(new byte[] { 1, 2, 3 }));
        Assert.That(uploadedOptions.HttpHeaders.ContentType, Is.EqualTo("application/vnd.ms-excel.sheet.macroEnabled.12"));
        Assert.That(uploadedOptions.Conditions.IfNoneMatch, Is.EqualTo(ETag.All));
        container.Verify(value => value.GetBlobClient("packs/request/inspection.xlsm"), Times.Once);
    }

    [Test]
    public void DownloadReturnsStorageStreamForCallerToDispose()
    {
        using var content = new MemoryStream(new byte[] { 4, 5, 6 });
        blob.Setup(value => value.DownloadStreaming(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(BlobsModelFactory.BlobDownloadStreamingResult(content: content), Mock.Of<Response>()));

        Assert.That(storage.Download("reports/request/report.html"), Is.SameAs(content));
    }

    [Test]
    public void MissingDownloadBecomesFileNotFound()
    {
        blob.Setup(value => value.DownloadStreaming(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .Throws(new RequestFailedException(404, "Offline missing file."));
        Assert.Throws<FileNotFoundException>(() => storage.Download("packs/request/inspection.xlsm"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DeleteIsIdempotentWhenBlobIsAlreadyAbsent(bool exists)
    {
        blob.Setup(value => value.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(exists, Mock.Of<Response>()));

        Assert.DoesNotThrow(() => storage.Delete("packs/request/inspection.xlsm"));
        blob.Verify(value => value.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(),
            It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase("upload")]
    [TestCase("download")]
    [TestCase("delete")]
    public void StorageAccessFailuresPropagateWithoutLocalFallback(string operation)
    {
        var error = new RequestFailedException(403, "Offline denied.");
        blob.Setup(value => value.Upload(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
            .Throws(error);
        blob.Setup(value => value.DownloadStreaming(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
            .Throws(error);
        blob.Setup(value => value.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .Throws(error);
        using var content = new MemoryStream(new byte[] { 1 });

        var actual = Assert.Throws<RequestFailedException>(() =>
        {
            if (operation == "upload") storage.Upload("packs/request/test.xlsm", content, "application/octet-stream");
            else if (operation == "download") storage.Download("packs/request/test.xlsm");
            else storage.Delete("packs/request/test.xlsm");
        });
        Assert.That(actual, Is.SameAs(error));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(@"C:\Uploads\pack.xlsm")]
    [TestCase(@"\\server\share\pack.xlsm")]
    [TestCase("https://storage.invalid/container/pack.xlsm")]
    [TestCase("/packs/request/pack.xlsm")]
    [TestCase("packs/../pack.xlsm")]
    [TestCase("packs//pack.xlsm")]
    [TestCase("packs/request/")]
    [TestCase("other/pack.xlsm")]
    public void InvalidReferencesCannotReachStorage(string name)
    {
        using var content = new MemoryStream();
        Assert.Throws<InvalidOperationException>(() => storage.Upload(name, content, "application/octet-stream"));
        Assert.Throws<InvalidOperationException>(() => storage.Download(name));
        Assert.Throws<InvalidOperationException>(() => storage.Delete(name));
        container.VerifyNoOtherCalls();
    }

    [TestCase(null)]
    [TestCase("33333333-3333-3333-3333-333333333333")]
    public void RegistrationUsesConfiguredContainerAndSingletonClientsWithoutNetwork(string clientId)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["BlobStorage:ServiceUri"] = "https://workbench-test.blob.core.windows.net/",
            ["BlobStorage:ContainerName"] = "private-files",
            ["BlobStorage:ManagedIdentityClientId"] = clientId
        }).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(Mock.Of<IHostEnvironment>(environment => environment.EnvironmentName == Environments.Production));
        services.AddWorkbenchBlobStorage(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetRequiredService<BlobContainerClient>().Uri,
            Is.EqualTo(new Uri("https://workbench-test.blob.core.windows.net/private-files")));
        Assert.That(provider.GetRequiredService<IFileStorage>(),
            Is.SameAs(provider.GetRequiredService<IFileStorage>()));
    }
}
