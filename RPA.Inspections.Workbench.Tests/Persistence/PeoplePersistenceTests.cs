using Microsoft.AspNetCore.Http;
using System.Data.Common;
using System.Data.Entity;
using NUnit.Framework;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Tests.Authentication;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
public class PeoplePersistenceTests
{
    private DbConnection connection;
    private PeopleContext context;

    [SetUp]
    public void SetUp()
    {
        connection = Effort.DbConnectionFactory.CreateTransient();
        context = new PeopleContext(connection, contextOwnsConnection: false);
        // Only the isolated in-memory test store is created; the external OLA database is never migrated.
        context.Database.Create();
    }

    [TearDown]
    public void TearDown()
    {
        context?.Dispose();
        connection?.Dispose();
    }

    [Test]
    public void StaffNumberAuditLookupAndSharedEmailAssignmentsUsePersistedPeople()
    {
        var location = new Location { id = Guid.NewGuid(), name = "Office", implementationEmail = "office@example.test" };
        context.People.AddRange(new[] {
            new Person { Id = Guid.NewGuid(), Name = "Inspector", StaffNumber = "M600300", Email = "inspector@example.test", Location = location },
            new Person { Id = Guid.NewGuid(), Name = "Second account", StaffNumber = "M600301", Email = "inspector@example.test", Location = location } });
        context.SaveChanges();
        context.DetachAll();
        var helper = new UserHelper(context, new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = AuthenticationTests.Principal() }
        });
        Assert.That(helper.CurrentUser(), Is.EqualTo("m600300"));
        Assert.That(helper.CurrentUserName(helper.CurrentUser()), Is.EqualTo("Inspector"));
        Assert.That(helper.CurrentUserEmail(helper.CurrentUser()), Is.EqualTo("inspector@example.test"));
        Assert.That(helper.GetAllStaffNumbersForCurrentUser(), Is.EquivalentTo(new[] { "M600300", "M600301" }));
    }

    [Test]
    public void CompositeManagerKeysAndPeopleLocationLazyLoadingRoundTrip()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(), Name = "Inspector", StaffNumber = "M123",
            Location = new Location { id = Guid.NewGuid(), name = "Office", implementationEmail = "office@example.test" }
        };
        context.Managers.AddRange(new[] {
            new Manager { Person = person, managerId = Guid.NewGuid() },
            new Manager { Person = person, managerId = Guid.NewGuid() } });
        context.SaveChanges();
        context.DetachAll();
        var loaded = context.People.Single();
        Assert.That(context.Entry(loaded).Reference(x => x.Location).IsLoaded, Is.False);
        Assert.That(loaded.Location.name, Is.EqualTo("Office"));
        Assert.That(loaded.Managers.Count, Is.EqualTo(2));
        Assert.That(loaded.Managers.All(x => x.personId == person.Id), Is.True);
        Assert.That(context.Managers.Select(x => x.managerId).Distinct().Count(), Is.EqualTo(2));
        Assert.That(typeof(PeopleContext).Assembly.GetTypes().Any(x =>
            !x.IsAbstract && typeof(System.Data.Entity.Migrations.DbMigration).IsAssignableFrom(x)), Is.False);
    }
}
