using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Validation;
using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
public class WorkbenchPersistenceTests
{
    private DbConnection connection;
    private WorkbenchContext context;

    [SetUp]
    public void SetUp()
    {
        connection = Effort.DbConnectionFactory.CreateTransient();
        context = new WorkbenchContext(connection, contextOwnsConnection: false);
        // Explicit creation is restricted to this transient provider. Production initializers stay disabled.
        context.Database.Create();
    }

    [TearDown]
    public void TearDown()
    {
        context?.Dispose();
        connection?.Dispose();
    }

    [Test]
    public void InvalidHoldingIsRejectedBeforeSaving()
    {
        context.Holding.Add(new Holding { cphNumber = "invalid", selectionMethod = "Random" });
        Assert.Throws<DbEntityValidationException>(() => context.SaveChanges());
        Assert.ThrowsAsync<DbEntityValidationException>(() => context.SaveChangesAsync());
        context.DetachAll();
        Assert.That(context.Holding.Any(), Is.False);
    }

    [Test]
    public async Task TrackedAndDetachedUpdatesPersistAndDeletedRowsStayDeleted()
    {
        var breed = new Breed { breedCode = "aa", breedFull = "Original" };
        context.Breed.Add(breed);
        await context.SaveChangesAsync();
        context.DetachAll();
        var loaded = context.Breed.Single();
        loaded.breedFull = "Tracked edit";
        context.SaveChanges();
        context.DetachAll();
        Assert.That(context.Breed.Single().breedFull, Is.EqualTo("Tracked edit"));
        context.DetachAll();
        context.SetModified(new Breed { breedId = breed.breedId, breedCode = "bb", breedFull = "Detached edit", Active = false });
        context.SaveChanges();
        context.DetachAll();
        loaded = context.Breed.Single();
        Assert.That(loaded.breedFull, Is.EqualTo("Detached edit"));
        Assert.That(loaded.breedCode, Is.EqualTo("BB"));
        Assert.That(loaded.Active, Is.False);
        context.Breed.Remove(loaded);
        context.SaveChanges();
        context.DetachAll();
        Assert.That(context.Breed.Any(), Is.False);
    }

    [Test]
    public void CattleQueriesLoadBreedForViewsAndSheetsAndRetainLazyLoading()
    {
        var holding = SeedInspection();
        context.DetachAll();
        var service = new CattleService(context);
        Assert.That(service.GetCattleByHoldingId(holding.holdingId).Single().Breed.breedFull, Is.EqualTo("Aberdeen"));
        context.DetachAll();
        Assert.That(service.GetLateCattle(holding.holdingId).Single().Breed.breedCode, Is.EqualTo("AA"));
        context.DetachAll();
        Assert.That(service.GetCattleByCPH(holding.cphNumber).Single().Breed.breedFull, Is.EqualTo("Aberdeen"));
        context.DetachAll();
        var cattle = context.Cattle.Single();
        Assert.That(context.Entry(cattle).Reference(x => x.Holding).IsLoaded, Is.False);
        Assert.That(cattle.Holding.cphNumber, Is.EqualTo(holding.cphNumber));
        Assert.That(context.Entry(cattle).Reference(x => x.Holding).IsLoaded, Is.True);
    }

    [Test]
    public void PackRequestCreateReadExpireAndHidePersistWithHoldingDetails()
    {
        var holding = SeedInspection();
        var user = new Mock<IUserHelper>();
        user.Setup(x => x.CurrentUser()).Returns("M123");
        user.Setup(x => x.CurrentUserName(It.IsAny<string>())).Returns("Inspector");
        user.Setup(x => x.CurrentUserEmail(It.IsAny<string>())).Returns("inspector@example.test");
        var service = new PackRequestedService(context, Mock.Of<IMailService>(), user.Object, Mock.Of<ISendService>(), Mock.Of<IFileStorage>());
        service.CreatePackRequested(holding.holdingId, holding.cphNumber);
        context.DetachAll();
        var request = context.PackRequested.Single();
        Assert.That(request.Id, Is.GreaterThan(0));
        Assert.That(request.packRequestedId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(request.requestedBy, Is.EqualTo("M123"));
        Assert.That(context.Holding.Single().lastDataRequest, Is.Not.Null);
        context.DetachAll();
        Assert.That(service.GetOutstandingPackRequests().Single().Holding.cphNumber, Is.EqualTo(holding.cphNumber));
        request = context.PackRequested.Single();
        request.packGenerated = DateTime.Now;
        request.activeRequest = false;
        context.SaveChanges();
        context.DetachAll();
        Assert.That(service.GetDownloadableRequests().Single().Holding.primaryName, Is.EqualTo("Test holding"));
        request = context.PackRequested.Single();
        request.packGenerated = DateTime.Now.AddDays(-4);
        request.activeRequest = true;
        context.SaveChanges();
        context.DetachAll();
        service.ThreeDayRule();
        context.DetachAll();
        Assert.That(context.PackRequested.Single().display, Is.False);
        Assert.That(context.PackRequested.Single().activeRequest, Is.False);
        Assert.That(service.GetDownloadableRequests(), Is.Empty);
    }

    [Test]
    public void RequiredDependentsCascadeButOptionalSchemeDoesNotDeleteHolding()
    {
        var holding = SeedInspection();
        context.PackRequested.Add(new PackRequested(holding.holdingId));
        context.LinkedHolding.Add(new LinkedHolding { linkedHoldingId = Guid.NewGuid(), Holding = holding });
        context.SaveChanges();
        var scheme = context.SchemeNameDrop.Single();
        context.SchemeNameDrop.Remove(scheme);
        context.SaveChanges();
        context.DetachAll();
        holding = context.Holding.Single();
        Assert.That(holding.SchemeNameDropId, Is.Null);
        context.Holding.Remove(holding);
        context.SaveChanges();
        context.DetachAll();
        Assert.That(context.Cattle.Any(), Is.False);
        Assert.That(context.PackRequested.Any(), Is.False);
        Assert.That(context.LinkedHolding.Any(), Is.False);
        Assert.That(context.Breed.Any(), Is.True);
    }

    [Test]
    public void BcmsControllerPersistsSpecificAndBulkRequests()
    {
        var controller = new BcmsAdminController(context, Mock.Of<IFileStorage>());
        var from = new DateTime(2026, 1, 1);
        var to = new DateTime(2026, 2, 1);
        controller.BulkRequest(from, to);
        controller.SpecificRequest("12/345/6789", 2026);
        context.DetachAll();
        var requests = context.BcmsRequested.ToList();
        Assert.That(requests, Has.Count.EqualTo(2));
        Assert.That(requests.Select(x => x.BcmsPackRequestedId).Distinct().Count(), Is.EqualTo(2));
        Assert.That(requests.All(x => x.ActiveRequest), Is.True);
        Assert.That(requests.Single(x => x.BulkRequest).StartDate, Is.EqualTo(from));
        Assert.That(requests.Single(x => !x.BulkRequest).SchemeYear, Is.EqualTo(2026));
    }

    private Holding SeedInspection()
    {
        var holding = new Holding
        {
            cphNumber = "12/345/6789", primaryName = "Test holding", SchemeName = "CII26", schemeYear = "2026", selectionMethod = "Random",
            SchemeNameDrop = new SchemeNameDrop { Text = "CII26", Active = true }
        };
        context.Cattle.Add(new Cattle
        {
            cattleId = Guid.NewGuid(), Holding = holding, Breed = new Breed { breedCode = "aa", breedFull = "Aberdeen" },
            listNumber = "LIST1", lateNotification = true, notificationDate = DateTime.Now,
            lateType = "Birth", numberOfDaysLate = 3
        });
        context.SaveChanges();
        Assert.That(holding.SchemeNameDropId, Is.GreaterThan(0));
        return holding;
    }
}
