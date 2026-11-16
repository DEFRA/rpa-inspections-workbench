using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.Tests.Authentication;

namespace RPA.Inspections.Workbench.Tests.Configuration;

[TestFixture]
public class KeyVaultConfigurationTests
{
    private static IHostEnvironment Environment() =>
        Mock.Of<IHostEnvironment>(environment => environment.EnvironmentName == Environments.Production);

    private static ConfigurationManager Configuration(string vaultUri = "https://workbench-test.vault.azure.net/")
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(AuthenticationTests.Settings());
        configuration["KeyVault:VaultUri"] = vaultUri;
        return configuration;
    }

    private static Dictionary<string, string> Secrets() => new()
    {
        ["ConnectionStrings--WorkbenchContext"] = "Server=unused;Database=VaultWorkbench;Integrated Security=true",
        ["ConnectionStrings--PeopleContext"] = "Server=unused;Database=VaultPeople;Integrated Security=true",
        ["AzureAd--ClientSecret"] = "offline-vault-test-only"
    };

    private static Mock<SecretClient> Client(Dictionary<string, string> secrets)
    {
        var client = new Mock<SecretClient>(MockBehavior.Strict);
        var properties = secrets.Keys.Select(name => new SecretProperties(name) { Enabled = true }).ToArray();
        client.Setup(value => value.GetPropertiesOfSecrets(It.IsAny<CancellationToken>()))
            .Returns(Pageable<SecretProperties>.FromPages(new[]
            {
                Page<SecretProperties>.FromValues(properties, null, Mock.Of<Response>())
            }));
        client.Setup(value => value.GetSecretAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SecretContentType?>(), It.IsAny<CancellationToken>()))
            .Returns((string name, string version, SecretContentType? contentType, CancellationToken cancellationToken) =>
                Task.FromResult(Response.FromValue(new KeyVaultSecret(name, secrets[name]), Mock.Of<Response>())));
        return client;
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void NoVaultUriPreservesExternalConfigurationWithoutContactingAzure(string vaultUri)
    {
        var configuration = Configuration(vaultUri);
        var client = new Mock<SecretClient>(MockBehavior.Strict);
        configuration.AddWorkbenchKeyVault(Environment(), client.Object);

        Assert.That(configuration.GetConnectionString("WorkbenchContext"),
            Is.EqualTo(AuthenticationTests.Settings()["ConnectionStrings:WorkbenchContext"]));
        client.VerifyNoOtherCalls();
    }

    [TestCase("http://workbench-test.vault.azure.net/")]
    [TestCase("not-a-uri")]
    [TestCase("https://workbench-test.vault.azure.net/secrets")]
    [TestCase("https://user:password@workbench-test.vault.azure.net/")]
    [TestCase("https://workbench-test.vault.azure.net/?query=value")]
    [TestCase("https://workbench-test.vault.azure.net/#fragment")]
    [TestCase("https://workbench-test.vault.azure.net:8443/")]
    public void InvalidVaultEndpointFailsBeforeContactingAzure(string vaultUri)
    {
        var configuration = Configuration(vaultUri);
        var client = new Mock<SecretClient>(MockBehavior.Strict);
        var error = Assert.Throws<InvalidOperationException>(() =>
            configuration.AddWorkbenchKeyVault(Environment(), client.Object));
        Assert.That(error.Message, Does.Contain("KeyVault:VaultUri"));
        client.VerifyNoOtherCalls();
    }

    [TestCase("not-a-guid")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public void InvalidManagedIdentityFailsBeforeContactingAzure(string clientId)
    {
        var configuration = Configuration();
        configuration["KeyVault:ManagedIdentityClientId"] = clientId;
        var client = new Mock<SecretClient>(MockBehavior.Strict);

        Assert.Throws<InvalidOperationException>(() =>
            configuration.AddWorkbenchKeyVault(Environment(), client.Object));
        client.VerifyNoOtherCalls();
    }

    [Test]
    public void ManagedIdentityWithoutVaultUriIsRejected()
    {
        var configuration = Configuration(null);
        configuration["KeyVault:ManagedIdentityClientId"] = "33333333-3333-3333-3333-333333333333";
        Assert.Throws<InvalidOperationException>(() => configuration.AddWorkbenchKeyVault(Environment()));
    }

    [TestCase(null)]
    [TestCase("33333333-3333-3333-3333-333333333333")]
    public void VaultSecretsOverrideEarlierSettingsAndConfigureBothDatabases(string clientId)
    {
        var configuration = Configuration();
        configuration["KeyVault:ManagedIdentityClientId"] = clientId;
        var secrets = Secrets();
        var client = Client(secrets);
        configuration.AddWorkbenchKeyVault(Environment(), client.Object);

        Assert.That(configuration["AzureAd:ClientSecret"], Is.EqualTo(secrets["AzureAd--ClientSecret"]));
        Assert.That(configuration.GetConnectionString("WorkbenchContext"),
            Is.EqualTo(secrets["ConnectionStrings--WorkbenchContext"]));
        Assert.That(configuration.GetConnectionString("PeopleContext"),
            Is.EqualTo(secrets["ConnectionStrings--PeopleContext"]));
        var services = new ServiceCollection().AddLogging();
        services.AddWorkbenchApplication(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.That(scope.ServiceProvider.GetRequiredService<WorkbenchContext>().Database.Connection.Database,
            Is.EqualTo("VaultWorkbench"));
        Assert.That(scope.ServiceProvider.GetRequiredService<PeopleContext>().Database.Connection.Database,
            Is.EqualTo("VaultPeople"));
    }

    [TestCase("WorkbenchContext", null)]
    [TestCase("PeopleContext", null)]
    [TestCase("WorkbenchContext", "")]
    [TestCase("PeopleContext", " ")]
    public void MissingOrBlankVaultConnectionCannotFallBackToEarlierSettings(string name, string value)
    {
        var secrets = Secrets();
        if (value == null)
            secrets.Remove("ConnectionStrings--" + name);
        else
            secrets["ConnectionStrings--" + name] = value;
        var configuration = Configuration();
        var client = Client(secrets);

        var error = Assert.Throws<InvalidOperationException>(() =>
            configuration.AddWorkbenchKeyVault(Environment(), client.Object));
        Assert.That(error.Message, Does.Contain("ConnectionStrings--" + name));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void VaultAccessFailuresStopStartupWithoutFallback(bool listingFailure)
    {
        var client = Client(Secrets());
        if (listingFailure)
        {
            client.Setup(value => value.GetPropertiesOfSecrets(It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(403, "Offline test: access denied."));
        }
        else
        {
            client.Setup(value => value.GetSecretAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SecretContentType?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(403, "Offline test: access denied."));
        }

        var configuration = Configuration();
        if (listingFailure)
        {
            Assert.Throws<RequestFailedException>(() =>
                configuration.AddWorkbenchKeyVault(Environment(), client.Object));
        }
        else
        {
            var error = Assert.Throws<AggregateException>(() =>
                configuration.AddWorkbenchKeyVault(Environment(), client.Object));
            Assert.That(error.Flatten().InnerExceptions, Is.All.TypeOf<RequestFailedException>());
        }
    }

    [TestCase("appsettings.json")]
    [TestCase("appsettings.Production.json")]
    [TestCase("appsettings.SIT.json")]
    [TestCase("appsettings.UAT.json")]
    public void ShippedSettingsDoNotContainSqlConnectionStrings(string fileName)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, fileName)).Build();
        Assert.That(configuration.GetSection("ConnectionStrings").GetChildren(), Is.Empty);
    }
}
