using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Services;
using NUnit.Framework;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;

namespace RPA.Inspections.Workbench.Tests.Pages
{
    [TestFixture]
    public class HoldingAndBcmsPagesTests
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;
        IHoldingService holdingService;
        IUserHelper userHelper;
        IPackRequested packRequestedService;
        Mock<IFileStorage> fileStorage;

        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            userHelper = new UserHelper(peopleContext.Context, MockHttpContext.GetMockedHttpContextAccessor());
            var filterService = new FilterService(context.Context, peopleContext.Context);
            holdingService = new HoldingService(context.Context, peopleContext.Context, filterService, userHelper);
            fileStorage = new Mock<IFileStorage>();
            packRequestedService = new PackRequestedService(context.Context, Mock.Of<IMailService>(), userHelper, Mock.Of<ISendService>(), fileStorage.Object);
        }

        [TearDown]
        public void TearDown()
        {
            context.Dispose();
            peopleContext.Dispose();
        }

        [Test]
        public void HoldingIndexSearchFindsHolding()
        {
            var page = new Workbench.Pages.Holding.IndexModel(holdingService, userHelper) { SearchString = "01/011/1001" }.WithContext("?page=1&pageSize=50");
            page.OnGet();
            Assert.AreEqual(1, page.Holdings.Count);
        }

        [Test]
        public void PacksRequestedListsOutstandingRequests()
        {
            var page = new Workbench.Pages.Holding.PacksRequestedModel(context.Context, packRequestedService);
            page.OnGet();
            Assert.AreEqual(1, page.Packs.Count);
        }

        [Test]
        public void DeletePackHidesPackAndRedirects()
        {
            var id = Guid.Parse("0C51DC76-B92D-420A-BA5F-006E564C06ED");
            var page = new Workbench.Pages.Holding.PacksRequestedModel(context.Context, packRequestedService);

            var result = page.OnPostDeletePack("TestPath", id);

            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.IsFalse(context.NewContext().PackRequested.Single(x => x.packRequestedId == id).display);
        }

        [Test]
        public void DeletePackRejectsPathsThatDoNotBelongToThePack()
        {
            var page = new Workbench.Pages.Holding.PacksRequestedModel(context.Context, packRequestedService);

            Assert.IsInstanceOf<NotFoundResult>(page.OnPostDeletePack("C:\\Windows\\win.ini", Guid.Parse("0C51DC76-B92D-420A-BA5F-006E564C06ED")));
            fileStorage.VerifyNoOtherCalls();
        }

        [Test]
        public void HoldingViewReturnsHoldingOrNotFound()
        {
            var page = new Workbench.Pages.Holding.ViewHoldingModel(holdingService, userHelper);
            Assert.IsInstanceOf<PageResult>(page.OnGet(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2")));
            Assert.IsInstanceOf<NotFoundResult>(page.OnGet(Guid.NewGuid()));
        }

        [Test]
        public void BcmsIndexListsRequests()
        {
            var page = new Workbench.Pages.BcmsAdmin.IndexModel(context.Context);
            page.OnGet();
            Assert.IsNotNull(page.Requests);
        }

        [Test]
        public void BcmsBulkRequestIsPersisted()
        {
            var before = context.NewContext().BcmsRequested.Count();

            new Workbench.Pages.BcmsAdmin.IndexModel(context.Context).OnPostBulk(new DateTime(2019, 1, 1), new DateTime(2019, 1, 2));

            var saved = context.NewContext().BcmsRequested.ToList();
            Assert.AreEqual(before + 1, saved.Count);
            Assert.That(saved.Any(x => x.BulkRequest && x.StartDate == new DateTime(2019, 1, 1) && x.ActiveRequest));
        }

        [Test]
        public void BcmsSpecificRequestIsPersisted()
        {
            new Workbench.Pages.BcmsAdmin.IndexModel(context.Context).OnPostSpecific("01/234/5678", 2018);

            Assert.That(context.NewContext().BcmsRequested.Any(x => x.CphNumber == "01/234/5678" && x.SchemeYear == 2018 && x.ActiveRequest));
        }
    }
}

