using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Tests.Authentication;
using RPA.Inspections.Workbench.Tests.DAL.Mock;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
public class PeoplePersistenceTests
{
    private MockPeopleContext db;
    private PeopleContext context;

    [SetUp]
    public void SetUp()
    {
        db = new MockPeopleContext(seed: false);
        context = db.Context;
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public void StaffNumberAuditLookupAndSharedEmailAssignmentsUsePersistedPeople()
    {
        var location = new Location { id = Guid.NewGuid(), name = "Office", implementationEmail = "office@example.test" };
        context.People.AddRange(new[] {
            new Person { Id = Guid.NewGuid(), Name = "Inspector", StaffNumber = "M600300", Email = "inspector@example.test", Location = location },
            new Person { Id = Guid.NewGuid(), Name = "Second account", StaffNumber = "M600301", Email = "inspector@example.test", Location = location } });
        context.SaveChanges();
        context.ChangeTracker.Clear();
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
    public void CompositeManagerKeysAndExplicitlyIncludedNavigationsRoundTrip()
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
        context.ChangeTracker.Clear();

        // Lazy loading is gone: navigations load only when included.
        var bare = context.People.Single();
        Assert.That(bare.Location, Is.Null);
        context.ChangeTracker.Clear();
        var loaded = context.People.Include(x => x.Location).Include(x => x.Managers).AsSplitQuery().Single();
        Assert.That(loaded.Location.name, Is.EqualTo("Office"));
        Assert.That(loaded.Managers.Count, Is.EqualTo(2));
        Assert.That(loaded.Managers.All(x => x.personId == person.Id), Is.True);
        Assert.That(context.Managers.Select(x => x.managerId).Distinct().Count(), Is.EqualTo(2));
    }
}
