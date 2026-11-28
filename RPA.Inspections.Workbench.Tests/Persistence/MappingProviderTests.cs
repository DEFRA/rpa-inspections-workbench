using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Tests.Authentication;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
public class MappingProviderTests
{
    private static readonly XNamespace Store = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";

    // Model metadata only: nothing connects to SQL Server.
    internal static IModel SqlServerModel<T>() where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>().UseSqlServer("Server=unused;Database=metadata_only;Integrated Security=true").Options;
        using var context = (T)Activator.CreateInstance(typeof(T), options);
        return context.GetService<IDesignTimeModel>().Model;
    }

    private static XElement ArchivedEf6Storage()
    {
        var resources = XDocument.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "EF6WorkbenchModel.resx"));
        var encoded = resources.Root.Elements("data").Single(x => (string)x.Attribute("name") == "Target").Element("value").Value;
        using var compressed = new MemoryStream(Convert.FromBase64String(encoded));
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        return XDocument.Load(gzip).Descendants(Store + "Schema").Single();
    }

    [Test]
    public void WorkbenchTablesColumnsKeysAndGenerationMatchArchivedEf6StorageModel()
    {
        var storage = ArchivedEf6Storage();
        var model = SqlServerModel<WorkbenchContext>();
        var archivedEntities = storage.Elements(Store + "EntityType").ToArray();
        Assert.That(model.GetEntityTypes().Count(), Is.EqualTo(archivedEntities.Length));

        foreach (var archived in archivedEntities)
        {
            var name = (string)archived.Attribute("Name");
            var set = storage.Descendants(Store + "EntitySet").Single(x => (string)x.Attribute("Name") == name);
            var entity = model.GetEntityTypes().Single(x => x.ClrType.Name == name);
            Assert.That(entity.GetSchema(), Is.EqualTo((string)set.Attribute("Schema")), name);
            Assert.That(entity.GetTableName(), Is.EqualTo((string)set.Attribute("Table") ?? name), name);

            var columns = archived.Elements(Store + "Property").ToArray();
            Assert.That(entity.GetProperties().Count(), Is.EqualTo(columns.Length), name);
            Assert.That(entity.FindPrimaryKey().Properties.Select(x => x.GetColumnName()),
                Is.EqualTo(archived.Element(Store + "Key").Elements(Store + "PropertyRef").Select(x => (string)x.Attribute("Name"))), name);

            foreach (var column in columns)
            {
                var columnName = (string)column.Attribute("Name");
                var property = entity.GetProperties().Single(x => x.GetColumnName() == columnName);
                var label = $"{name}.{columnName}";
                var archivedType = (string)column.Attribute("Type");
                Assert.That(archivedType.EndsWith("(max)") ? property.GetColumnType() : property.GetColumnType().Split('(')[0], Is.EqualTo(archivedType), label);
                Assert.That(property.IsColumnNullable(), Is.EqualTo((string)column.Attribute("Nullable") != "false"), label);
                var maxLength = (string)column.Attribute("MaxLength");
                if (int.TryParse(maxLength, out var length))
                    Assert.That(property.GetMaxLength(), Is.EqualTo(length), label);
                if ((string)column.Attribute("Type") == "decimal")
                {
                    Assert.That(property.GetPrecision(), Is.EqualTo((int)column.Attribute("Precision")), label);
                    Assert.That(property.GetScale(), Is.EqualTo((int)column.Attribute("Scale")), label);
                }
                var identity = (string)column.Attribute("StoreGeneratedPattern") == "Identity";
                Assert.That(SqlServerPropertyExtensions.GetValueGenerationStrategy(property) == SqlServerValueGenerationStrategy.IdentityColumn, Is.EqualTo(identity), label);
            }
        }
    }

    [Test]
    public void WorkbenchForeignKeysRetainRequiredCascadeAndOptionalNoCascade()
    {
        var keys = SqlServerModel<WorkbenchContext>().GetEntityTypes().SelectMany(x => x.GetForeignKeys()).ToArray();
        Assert.That(keys, Has.Length.EqualTo(5));
        foreach (var key in keys)
        {
            Assert.That(key.Properties, Has.Count.EqualTo(1));
            if (key.DeclaringEntityType.ClrType == typeof(Holding))
            {
                Assert.That(key.IsRequired, Is.False);
                Assert.That(key.DeleteBehavior, Is.Not.EqualTo(DeleteBehavior.Cascade));
            }
            else
            {
                Assert.That(key.IsRequired, Is.True);
                Assert.That(key.DeleteBehavior, Is.EqualTo(DeleteBehavior.Cascade));
            }
        }
        var request = SqlServerModel<WorkbenchContext>().FindEntityType(typeof(PackRequested));
        Assert.That(request.FindPrimaryKey().Properties.Select(x => x.Name), Is.EqualTo(new[] { "packRequestedId" }));
        Assert.That(request.FindProperty("packRequestedId").ValueGenerated, Is.EqualTo(ValueGenerated.Never));
    }

    [Test]
    public void PeopleUsesSeparateOlaSchemaCompositeKeyAndExternalOwnership()
    {
        var model = SqlServerModel<PeopleContext>();
        Assert.That(model.GetEntityTypes().Count(), Is.EqualTo(3));
        foreach (var entity in model.GetEntityTypes())
        {
            Assert.That(entity.GetSchema(), Is.EqualTo("OLA"));
            Assert.That(entity.GetTableName(), Does.Not.Contain("."));
        }
        var manager = model.FindEntityType(typeof(Manager));
        Assert.That(manager.FindPrimaryKey().Properties.Select(x => x.GetColumnName()), Is.EqualTo(new[] { "personId", "managerId" }));
        var person = model.FindEntityType(typeof(Person));
        Assert.That(person.GetProperties().Select(x => x.GetColumnName()), Does.Contain("id"));
        Assert.That(person.FindProperty(nameof(Person.StaffNumber)).GetMaxLength(), Is.EqualTo(10));
        Assert.That(person.GetProperties().Single(x => x.GetColumnName() == "updated").GetColumnType(), Is.EqualTo("datetime"));
        Assert.That(model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()).Select(x => x.DeleteBehavior),
            Is.All.EqualTo(DeleteBehavior.Cascade));
        Assert.That(typeof(PeopleContext).Assembly.GetTypes().Any(x => x.Name.EndsWith("ModelSnapshot")), Is.False);
    }

    [Test]
    public void DependencyInjectionSharesOneContextPerScopeAndUsesSqlServer()
    {
        using var provider = AuthenticationTests.Provider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var workbench = first.ServiceProvider.GetRequiredService<WorkbenchContext>();
        var people = first.ServiceProvider.GetRequiredService<PeopleContext>();
        Assert.That(first.ServiceProvider.GetRequiredService<IWorkbenchContext>(), Is.SameAs(workbench));
        Assert.That(first.ServiceProvider.GetRequiredService<IPeopleContext>(), Is.SameAs(people));
        Assert.That(second.ServiceProvider.GetRequiredService<WorkbenchContext>(), Is.Not.SameAs(workbench));
        Assert.That(second.ServiceProvider.GetRequiredService<PeopleContext>(), Is.Not.SameAs(people));
        Assert.That(workbench.Database.ProviderName, Is.EqualTo("Microsoft.EntityFrameworkCore.SqlServer"));
        Assert.That(people.Database.ProviderName, Is.EqualTo("Microsoft.EntityFrameworkCore.SqlServer"));
        Assert.That(workbench.Database.GetDbConnection().State, Is.EqualTo(System.Data.ConnectionState.Closed));
        Assert.That(workbench.ChangeTracker.AutoDetectChangesEnabled, Is.True);
        Assert.That(typeof(WorkbenchContext).Assembly.GetReferencedAssemblies().Any(x => x.Name == "EntityFramework"), Is.False);
    }

    [Test]
    public void OldStartupMigrationSwitchFailsClosedBeforeDatabaseAccess()
    {
        using var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("ApplyMigrationsOnStartup", "true"));
        var error = Assert.Throws<InvalidOperationException>(() => application.CreateClient());
        Assert.That(error.Message, Does.Contain("existing-schema deployment procedure"));
    }

    [Test]
    public void SqlServerTranslatesRepresentativeQueriesWithoutOpeningAConnection()
    {
        var options = new DbContextOptionsBuilder<WorkbenchContext>().UseSqlServer("Server=unused;Database=metadata_only;Integrated Security=true").Options;
        using var workbench = new WorkbenchContext(options);
        Assert.That(workbench.Holding.Where(Extensions.YearExtensions.MatchesReference("2026")).ToQueryString(), Does.Contain("SUBSTRING"));
        Assert.That(workbench.Cattle.Include(x => x.Breed)
            .Where(x => x.lateNotification && x.notificationDate.Value.Year == 2026)
            .OrderBy(x => x.lateType).ThenByDescending(x => x.numberOfDaysLate).ToQueryString(), Does.Contain("DATEPART"));
        Assert.That(workbench.PackRequested.Include(x => x.Holding)
            .Where(x => x.packGenerated != null && x.requestedBy.ToUpper() == "M123" && x.display)
            .OrderByDescending(x => x.packGenerated).ToQueryString(), Does.Contain("JOIN"));
        Assert.That(workbench.Breed.Where(x => x.breedCode.ToUpper().Contains("AB")).ToQueryString(), Does.Contain("UPPER"));
        Assert.That(workbench.Database.GetDbConnection().State, Is.EqualTo(System.Data.ConnectionState.Closed));
    }
}



