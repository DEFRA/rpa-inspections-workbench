using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using RPA.Inspections.Workbench.Helpers;
using NUnit.Framework;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Pages.Admin;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;
using X.PagedList;

namespace RPA.Inspections.Workbench.Tests.Pages
{
    [TestFixture]
    public class AdminPagesTests
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;
        IHoldingService holdingService;
        IBreedService breedService;

        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            var userHelper = new UserHelper(peopleContext.Context, MockHttpContext.GetMockedHttpContextAccessor());
            var filterService = new FilterService(context.Context, peopleContext.Context);
            holdingService = new HoldingService(context.Context, peopleContext.Context, filterService, userHelper);
            breedService = new BreedService(context.Context);
        }

        [TearDown]
        public void TearDown()
        {
            context.Dispose();
            peopleContext.Dispose();
        }

        [Test]
        public void BreedListPagesAndSearchesFromTheQueryString()
        {
            var all = new BreedListModel(breedService).WithContext();
            all.OnGet();
            Assert.AreEqual(4, all.Breeds.Count);

            var search = new BreedListModel(breedService) { SearchString = "Mega Cow 1" }.WithContext("?page=1&pageSize=50");
            search.OnGet();
            Assert.AreEqual(1, search.Breeds.Count);
        }

        [Test]
        public void ViewBreedGetReturnsBreedOrNotFound()
        {
            var page = new ViewBreedModel(context.Context, breedService);
            Assert.IsInstanceOf<PageResult>(page.OnGet(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79")));
            Assert.AreEqual("Mega Cow", page.Breed.breedFull);
            Assert.IsInstanceOf<NotFoundResult>(page.OnGet(Guid.NewGuid()));
        }

        [Test]
        public void AddBreedPersistsBreed()
        {
            var page = new AddBreedModel(context.Context)
            {
                Breed = new Breed { breedId = Guid.NewGuid(), breedCode = "BFY", breedFull = "Beefy", Active = true }
            };

            var result = page.OnPost();

            Assert.IsInstanceOf<RedirectToPageResult>(result);
            Assert.AreEqual(5, context.NewContext().Breed.Count());
        }

        [Test]
        public void ViewBreedPostSavesChanges()
        {
            var id = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79");
            var breed = context.NewContext().Breed.AsNoTracking().Single(x => x.breedId == id);
            breed.breedFull = "Mega Mega Cow";

            new ViewBreedModel(context.Context, breedService) { Breed = breed }.OnPost();

            Assert.AreEqual("Mega Mega Cow", context.NewContext().Breed.Single(x => x.breedId == id).breedFull);
        }

        [Test]
        public void HoldingListSearchFindsHolding()
        {
            var page = new HoldingListModel(holdingService) { SearchString = "01/011/1001" }.WithContext();
            page.OnGet();
            Assert.AreEqual(1, page.Holdings.Count);
        }

        [Test]
        public void ArchivedListFiltersByYearAndSearch()
        {
            var byYear = new ArchivedListModel(context.Context, holdingService) { SearchYear = "2017" }.WithContext();
            byYear.OnGet();
            Assert.AreEqual(2, byYear.Holdings.Count);

            var bySearch = new ArchivedListModel(context.Context, holdingService) { SearchString = "01/011/1005" }.WithContext();
            bySearch.OnGet();
            Assert.AreEqual(1, bySearch.Holdings.Count);
        }

        [Test]
        public void ViewArchivedHoldingReturnsHoldingOrNotFound()
        {
            var page = new ViewArchivedHoldingModel(holdingService);
            Assert.IsInstanceOf<PageResult>(page.OnGet(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2")));
            Assert.IsNotNull(page.Holding);
            Assert.IsInstanceOf<NotFoundResult>(page.OnGet(Guid.NewGuid()));
        }

        [Test]
        public void AddHoldingPersistsHoldingAndDerivesSchemeFromSchemeName()
        {
            var page = new AddHoldingModel(context.Context)
            {
                Holding = new Holding
                {
                    holdingId = Guid.NewGuid(), Active = true, animalsToBeInspected = 50, cphNumber = "88/888/8888",
                    AssignedUser = "M600300", inspectionDeadlineDate = DateTime.Now, SBI = 123456789, SchemeNameDropId = 1, selectionMethod = "RANDOM"
                }
            }.WithContext();

            var result = page.OnPost();

            Assert.IsInstanceOf<RedirectToPageResult>(result);
            var saved = context.NewContext().Holding.Single(x => x.cphNumber == "88/888/8888");
            Assert.AreEqual(7, context.NewContext().Holding.Count());
            Assert.AreEqual(saved.SchemeName.Substring(saved.SchemeName.Length - 2), saved.schemeYear.Substring(2));
        }

        [Test]
        public void AddHoldingWithUnknownSchemeNameReturnsPageWithModelError()
        {
            var page = new AddHoldingModel(context.Context) { Holding = new Holding { SchemeNameDropId = 999 } }.WithContext();

            Assert.IsInstanceOf<PageResult>(page.OnPost());
            Assert.IsFalse(page.ModelState.IsValid);
        }

        [Test]
        public void ViewHoldingPostSavesEdits()
        {
            var id = Guid.Parse("9548D01E-15FC-4CB5-97AB-000EB6EFD6C3");
            var holding = context.NewContext().Holding.AsNoTracking().Single(x => x.holdingId == id);
            holding.primaryName = "Potato Farm";

            var result = new ViewHoldingModel(context.Context, holdingService) { Holding = holding }.WithContext().OnPost();

            Assert.IsNotInstanceOf<NotFoundResult>(result);
            Assert.AreEqual("Potato Farm", context.NewContext().Holding.Single(x => x.holdingId == id).primaryName);
        }

        [Test]
        public void InspectorListShowsRemovesAndRejectsUnknownInspector()
        {
            var page = new InspectorListModel(context.Context);
            page.OnGet();
            Assert.AreEqual(1, page.Inspectors.Count);

            Assert.IsInstanceOf<NotFoundResult>(page.OnPostRemove(Guid.NewGuid()));
            Assert.IsInstanceOf<RedirectToPageResult>(page.OnPostRemove(page.Inspectors[0].inspectorId));
            Assert.AreEqual(0, context.NewContext().Inspector.Count());
        }

        [Test]
        public void AddInspectorAddsOnlyNewStaffNumbers()
        {
            new AddInspectorModel(context.Context) { Inspector = new Inspector { inspectorId = Guid.NewGuid(), staffNumber = "M100100" } }.OnPost();
            Assert.AreEqual(2, context.NewContext().Inspector.Count());

            new AddInspectorModel(context.Context) { Inspector = new Inspector { inspectorId = Guid.NewGuid(), staffNumber = "M600300" } }.OnPost();
            Assert.AreEqual(2, context.NewContext().Inspector.Count());
        }

        [Test]
        public void SchemeYearAdminPostUpdatesControlsAndSchemeNames()
        {
            var page = new SchemeYearAdminModel(context.Context);
            page.OnGet();
            var controls = page.Controls;
            controls.Single(x => x.Value == "2019").Active = false;

            new SchemeYearAdminModel(context.Context) { Controls = controls }.OnPost();

            var fresh = context.NewContext();
            Assert.AreEqual(false, fresh.Control.Single(x => x.Value == "2019").Active);
            Assert.That(fresh.SchemeNameDrop.Where(x => x.Text.EndsWith("19")).All(x => x.Active == false));
        }

        [Test]
        public void SchemeYearAdminAddNextAddsNextYearAndSchemeNames()
        {
            var before = context.NewContext();
            var controls = before.Control.Count();
            var names = before.SchemeNameDrop.Count();

            var result = new SchemeYearAdminModel(context.Context).OnPostAddNext();

            Assert.IsInstanceOf<RedirectToPageResult>(result);
            var after = context.NewContext();
            Assert.AreEqual(controls + 1, after.Control.Count());
            Assert.AreEqual(names + 3, after.SchemeNameDrop.Count());
        }

        [Test]
        public async Task BulkUploadRequiresSuperUserAndReportsFailures()
        {
            var fileService = new Mock<IFileService>();
            var authorization = new Mock<IAuthorizationService>();
            authorization.Setup(x => x.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), null, ApplicationServices.SuperUserPolicy))
                .ReturnsAsync(AuthorizationResult.Failed());
            var denied = new IndexModel(fileService.Object, authorization.Object).WithContext();
            Assert.IsInstanceOf<ForbidResult>(await denied.OnPostAsync(Mock.Of<IFormFile>(), true));
            fileService.Verify(x => x.RunAddList(It.IsAny<IFormFile>(), It.IsAny<bool>()), Times.Never);

            authorization.Setup(x => x.AuthorizeAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), null, ApplicationServices.SuperUserPolicy))
                .ReturnsAsync(AuthorizationResult.Success());
            var allowed = new IndexModel(fileService.Object, authorization.Object).WithContext();
            Assert.IsInstanceOf<RedirectToPageResult>(await allowed.OnPostAsync(Mock.Of<IFormFile>(), true));

            fileService.Setup(x => x.RunAddList(It.IsAny<IFormFile>(), It.IsAny<bool>())).Throws(new InvalidOperationException("bad file"));
            var failed = new IndexModel(fileService.Object, authorization.Object).WithContext();
            var result = await failed.OnPostAsync(Mock.Of<IFormFile>(), false) as RedirectToPageResult;
            Assert.AreEqual("FileLoadFail", result.PageName);
            Assert.AreEqual("bad file", failed.TempData["ErrorMessage"]);
        }
    }
}



