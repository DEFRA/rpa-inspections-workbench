using Microsoft.EntityFrameworkCore;
using Moq;
using RPA.Inspections.Workbench.DAL;
using NUnit.Framework;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using System.ComponentModel.DataAnnotations;

namespace RPA.Inspections.Workbench.Tests.Persistence;

[TestFixture]
public class WorkbenchPersistenceTests
{
    private MockWorkbenchContext db;
    private WorkbenchContext context;

    [SetUp]
    public void SetUp()
    {
        db = new MockWorkbenchContext(seed: false, enforceForeignKeys: true);
        context = db.Context;
    }

    [TearDown]
    public void TearDown() => db.Dispose();

    [Test]
    public void InvalidHoldingIsRejectedBeforeSaving()
    {
        context.Holding.Add(new Holding { cphNumber = "invalid", selectionMethod = "Random" });
        Assert.Throws<ValidationException>(() => context.SaveChanges());
        Assert.ThrowsAsync<ValidationException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();
        Assert.That(context.Holding.Any(), Is.False);
    }

    [Test]
    public async Task TrackedAndDetachedUpdatesPersistAndDeletedRowsStayDeleted()
    {
        var breed = new Breed { breedCode = "aa", breedFull = "Original" };
        context.Breed.Add(breed);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var loaded = context.Breed.Single();
        loaded.breedFull = "Tracked edit";
        context.SaveChanges();
        context.ChangeTracker.Clear();
        Assert.That(context.Breed.Single().breedFull, Is.EqualTo("Tracked edit"));
        context.ChangeTracker.Clear();
        context.SetModified(new Breed { breedId = breed.breedId, breedCode = "bb", breedFull = "Detached edit", Active = false });
        context.SaveChanges();
        context.ChangeTracker.Clear();
        loaded = context.Breed.Single();
        Assert.That(loaded.breedFull, Is.EqualTo("Detached edit"));
        Assert.That(loaded.breedCode, Is.EqualTo("BB"));
        Assert.That(loaded.Active, Is.False);
        context.Breed.Remove(loaded);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        Assert.That(context.Breed.Any(), Is.False);
    }

    [Test]
    public void CattleQueriesLoadBreedForViewsAndSheetsAndDoNotLazyLoad()
    {
        var holding = SeedInspection();
        context.ChangeTracker.Clear();
        var service = new CattleService(context);
        Assert.That(service.GetCattleByHoldingId(holding.holdingId).Single().Breed.breedFull, Is.EqualTo("Aberdeen"));
        context.ChangeTracker.Clear();
        Assert.That(service.GetLateCattle(holding.holdingId).Single().Breed.breedCode, Is.EqualTo("AA"));
        context.ChangeTracker.Clear();
        Assert.That(service.GetCattleByCPH(holding.cphNumber).Single().Breed.breedFull, Is.EqualTo("Aberdeen"));
        context.ChangeTracker.Clear();
        var cattle = context.Cattle.Single();
        Assert.That(cattle.Holding, Is.Null);
        Assert.That(context.Entry(cattle).Reference(x => x.Holding).IsLoaded, Is.False);
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
        context.ChangeTracker.Clear();
        var request = context.PackRequested.Single();
        Assert.That(request.packRequestedId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(request.requestedBy, Is.EqualTo("M123"));
        Assert.That(context.Holding.Single().lastDataRequest, Is.Not.Null);
        context.ChangeTracker.Clear();
        Assert.That(service.GetOutstandingPackRequests().Single().Holding.cphNumber, Is.EqualTo(holding.cphNumber));
        request = context.PackRequested.Single();
        request.packGenerated = DateTime.Now;
        request.activeRequest = false;
        context.SaveChanges();
        context.ChangeTracker.Clear();
        Assert.That(service.GetDownloadableRequests().Single().Holding.primaryName, Is.EqualTo("Test holding"));
        request = context.PackRequested.Single();
        request.packGenerated = DateTime.Now.AddDays(-4);
        request.activeRequest = true;
        context.SaveChanges();
        context.ChangeTracker.Clear();
        service.ThreeDayRule();
        context.ChangeTracker.Clear();
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
        context.ChangeTracker.Clear();
        holding = context.Holding.Single();
        Assert.That(holding.SchemeNameDropId, Is.Null);
        context.Holding.Remove(holding);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        Assert.That(context.Cattle.Any(), Is.False);
        Assert.That(context.PackRequested.Any(), Is.False);
        Assert.That(context.LinkedHolding.Any(), Is.False);
        Assert.That(context.Breed.Any(), Is.True);
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



