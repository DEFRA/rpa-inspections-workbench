using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Tests.Authentication;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
public class MappingProviderTests
{
    [Test]
    public void WorkbenchTablesColumnsKeysAndGenerationMatchArchivedEf6StorageModel()
    {
        var resources = XDocument.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "EF6WorkbenchModel.resx"));
        var encoded = resources.Root.Elements("data").Single(x => (string)x.Attribute("name") == "Target").Element("value").Value;
        using var compressed = new MemoryStream(Convert.FromBase64String(encoded));
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        var archived = XDocument.Load(gzip);
        var ns = Ef6TestModel.StoreNamespace;
        var storage = archived.Descendants(ns + "Schema").Single();
        var current = Ef6TestModel.Storage<WorkbenchContext>();
        var entities = current.Elements(ns + "EntityType").ToArray();
        Assert.That(entities, Has.Length.EqualTo(storage.Elements(ns + "EntityType").Count()));

        foreach (var oldEntity in storage.Elements(ns + "EntityType"))
        {
            var name = (string)oldEntity.Attribute("Name");
            var entity = entities.Single(x => (string)x.Attribute("Name") == name);
            var table = storage.Descendants(ns + "EntitySet").Single(x => (string)x.Attribute("Name") == name);
            var currentTable = current.Descendants(ns + "EntitySet").Single(x => (string)x.Attribute("Name") == name);
            Assert.That((string)currentTable.Attribute("Schema"), Is.EqualTo((string)table.Attribute("Schema")), name);
            Assert.That((string)currentTable.Attribute("Table"), Is.EqualTo((string)table.Attribute("Table")), name);
            var columns = oldEntity.Elements(ns + "Property").ToArray();
            Assert.That(entity.Elements(ns + "Property").Count(), Is.EqualTo(columns.Length), name);
            Assert.That(entity.Element(ns + "Key").Elements(ns + "PropertyRef").Select(x => (string)x.Attribute("Name")),
                Is.EqualTo(oldEntity.Element(ns + "Key").Elements(ns + "PropertyRef").Select(x => (string)x.Attribute("Name"))), name);
            foreach (var column in columns)
            {
                var columnName = (string)column.Attribute("Name");
                var property = entity.Elements(ns + "Property").Single(x => (string)x.Attribute("Name") == columnName);
                foreach (var facet in new[] { "Type", "MaxLength", "Precision", "Scale", "Unicode", "FixedLength", "StoreGeneratedPattern" })
                    Assert.That((string)property.Attribute(facet), Is.EqualTo((string)column.Attribute(facet)), $"{name}.{columnName}.{facet}");
                Assert.That((string)property.Attribute("Nullable") != "false",
                    Is.EqualTo((string)column.Attribute("Nullable") != "false"), $"{name}.{columnName}.Nullable");
            }
        }
    }

    [Test]
    public void WorkbenchForeignKeysRetainEf6RequiredCascadeAndOptionalNoCascade()
    {
        var ns = Ef6TestModel.StoreNamespace;
        var storage = Ef6TestModel.Storage<WorkbenchContext>();
        var keys = storage.Elements(ns + "Association").ToArray();
        Assert.That(keys, Has.Length.EqualTo(5));
        foreach (var key in keys)
        {
            var constraint = key.Element(ns + "ReferentialConstraint");
            var dependent = constraint.Element(ns + "Dependent");
            Assert.That(dependent.Elements(ns + "PropertyRef").Count(), Is.EqualTo(1));
            var principal = key.Elements(ns + "End").Single(x =>
                (string)x.Attribute("Role") == (string)constraint.Element(ns + "Principal").Attribute("Role"));
            Assert.That((string)principal.Element(ns + "OnDelete")?.Attribute("Action"),
                Is.EqualTo((string)dependent.Attribute("Role") == "Holding" ? null : "Cascade"));
        }
        var request = storage.Elements(ns + "EntityType").Single(x => (string)x.Attribute("Name") == nameof(PackRequested));
        Assert.That((string)request.Elements(ns + "Property").Single(x => (string)x.Attribute("Name") == "Id")
            .Attribute("StoreGeneratedPattern"), Is.EqualTo("Identity"));
        Assert.That(request.Element(ns + "Key").Elements(ns + "PropertyRef").Select(x => (string)x.Attribute("Name")),
            Is.EqualTo(new[] { "packRequestedId" }));
        foreach (var guid in storage.Elements(ns + "EntityType").SelectMany(x => x.Elements(ns + "Property"))
                     .Where(x => (string)x.Attribute("Type") == "uniqueidentifier"))
            Assert.That((string)guid.Attribute("StoreGeneratedPattern"), Is.Null, (string)guid.Attribute("Name"));
    }

    [Test]
    public void PeopleUsesSeparateOlaSchemaCompositeKeyAndExternalOwnership()
    {
        var ns = Ef6TestModel.StoreNamespace;
        var storage = Ef6TestModel.Storage<PeopleContext>();
        Assert.That(storage.Elements(ns + "EntityType").Count(), Is.EqualTo(3));
        foreach (var entity in storage.Descendants(ns + "EntitySet"))
        {
            Assert.That((string)entity.Attribute("Schema"), Is.EqualTo("OLA"));
            Assert.That((string)entity.Attribute("Table"), Does.Not.Contain("."));
        }
        var manager = storage.Elements(ns + "EntityType").Single(x => (string)x.Attribute("Name") == "Manager");
        Assert.That(manager.Element(ns + "Key").Elements(ns + "PropertyRef").Select(x => (string)x.Attribute("Name")),
            Is.EqualTo(new[] { "personId", "managerId" }));
        var person = storage.Elements(ns + "EntityType").Single(x => (string)x.Attribute("Name") == "Person");
        Assert.That(person.Elements(ns + "Property").Select(x => (string)x.Attribute("Name")), Does.Contain("id"));
        Assert.That((string)person.Elements(ns + "Property").Single(x => (string)x.Attribute("Name") == "staffNumber")
            .Attribute("MaxLength"), Is.EqualTo("10"));
        Assert.That((string)person.Elements(ns + "Property").Single(x => (string)x.Attribute("Name") == "updated")
            .Attribute("Type"), Is.EqualTo("datetime"));
        Assert.That(storage.Elements(ns + "Association").SelectMany(x => x.Elements(ns + "End"))
            .Where(x => x.Element(ns + "OnDelete") != null).Select(x => (string)x.Element(ns + "OnDelete").Attribute("Action")),
            Is.EqualTo(new[] { "Cascade", "Cascade" }));
        Assert.That(typeof(PeopleContext).Assembly.GetTypes().Any(x => !x.IsAbstract && typeof(DbMigration).IsAssignableFrom(x)), Is.False);
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
        Assert.That(workbench.Database.Connection, Is.TypeOf<SqlConnection>());
        Assert.That(people.Database.Connection, Is.TypeOf<SqlConnection>());
        Assert.That(workbench.Database.Connection.State, Is.EqualTo(System.Data.ConnectionState.Closed));
        Assert.That(people.Database.Connection.State, Is.EqualTo(System.Data.ConnectionState.Closed));
        Assert.That(workbench.Configuration.AutoDetectChangesEnabled, Is.True);
        Assert.That(workbench.Configuration.ValidateOnSaveEnabled, Is.True);
        Assert.That(people.Configuration.AutoDetectChangesEnabled, Is.True);
        Assert.That(people.Configuration.ValidateOnSaveEnabled, Is.True);
        Assert.That(typeof(WorkbenchContext).Assembly.GetReferencedAssemblies().Any(x => x.Name == "EntityFramework"), Is.True);
        Assert.That(typeof(WorkbenchContext).Assembly.GetReferencedAssemblies().Any(x => x.Name.StartsWith("Microsoft.EntityFrameworkCore")), Is.False);
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
    public void ArchivedMigrationsAndEfCoreBaselineAreNotExecutable()
    {
        var assembly = typeof(WorkbenchContext).Assembly;
        Assert.That(assembly.GetTypes().Any(x => !x.IsAbstract && typeof(DbMigration).IsAssignableFrom(x)), Is.False);
        Assert.That(assembly.GetTypes().Any(x => x.Name == "ExistingSchemaBaseline" || x.Name.EndsWith("ModelSnapshot")), Is.False);
        Assert.That(typeof(WorkbenchContext).GetCustomAttributes(typeof(DbConfigurationTypeAttribute), true), Has.Length.EqualTo(1));
        Assert.That(typeof(PeopleContext).GetCustomAttributes(typeof(DbConfigurationTypeAttribute), true), Has.Length.EqualTo(1));
    }

    [Test]
    public void SqlServerTranslatesRepresentativeQueriesWithoutOpeningAConnection()
    {
        using var workbench = Ef6TestModel.QueryContext<WorkbenchContext>();
        using var people = Ef6TestModel.QueryContext<PeopleContext>();
        Assert.That(workbench.Set<Holding>().Where(Extensions.YearExtensions.MatchesReference("2026")).ToString(), Does.Contain("SUBSTRING"));
        Assert.That(workbench.Set<Cattle>().Include(x => x.Breed)
            .Where(x => x.lateNotification && x.notificationDate.Value.Year == 2026)
            .OrderBy(x => x.lateType).ThenByDescending(x => x.numberOfDaysLate).ToString(), Does.Contain("DATEPART"));
        Assert.That(workbench.Set<PackRequested>().Include(x => x.Holding)
            .Where(x => x.packGenerated != null && x.requestedBy.ToUpper() == "M123" && x.display)
            .OrderByDescending(x => x.packGenerated).ToString(), Does.Contain("JOIN"));
        Assert.That(people.Set<Person>().Where(x => x.StaffNumber.ToLower() == "m123").Select(x => x.Email).ToString(),
            Does.Contain("[OLA].[People]"));
        Assert.That(workbench.Set<Breed>().Where(x => x.breedCode.ToUpper().Contains("AB") || x.breedFull.ToUpper().Contains("AB")).ToString(),
            Does.Contain("UPPER"));
        Assert.That(workbench.Database.Connection.State, Is.EqualTo(System.Data.ConnectionState.Closed));
        Assert.That(people.Database.Connection.State, Is.EqualTo(System.Data.ConnectionState.Closed));
    }
}
