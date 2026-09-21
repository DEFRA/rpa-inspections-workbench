using Moq;
using NUnit.Framework;
using PagedList;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.Factory;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Tests.Controllers
{
    [TestFixture]

    public class HoldingControllerTest
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;
        HoldingController controller;

        IHoldingService holdingService;
        ILinkedHoldingService linkedHoldingService;
        ICattleService cattleService;
        IPackRequested packRequestedService;
        Mock<IFileService> fileService;
        Mock<IMailService> mailService;
        Mock<IHolidayService> holidayService;
        Mock<ISendService> sendService;
        IFilterService filterService;
        IUserHelper userHelper;

        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();

            userHelper = new UserHelper(peopleContext.MockContext.Object);
            filterService = new FilterService(context.MockContext.Object, peopleContext.MockContext.Object);
            holdingService = new HoldingService(context.MockContext.Object, peopleContext.MockContext.Object, filterService, userHelper);
            linkedHoldingService = new LinkedHoldingService(context.MockContext.Object);
            cattleService = new CattleService(context.MockContext.Object);
            sendService = new Mock<ISendService>();
            fileService = new Mock<IFileService>();
            mailService = new Mock<IMailService>();
            holidayService = new Mock<IHolidayService>();
            packRequestedService = new PackRequestedService(context.MockContext.Object, mailService.Object, userHelper, sendService.Object);

            HttpContextManager.SetCurrentContext(MockHttpContext.GetMockedHttpContext());

            controller = new HoldingController(context.MockContext.Object, peopleContext.MockContext.Object, holdingService, linkedHoldingService, cattleService, packRequestedService, mailService.Object, fileService.Object, filterService, userHelper, sendService.Object, holidayService.Object);

        }

        [Test]

        public void Test_Index_Returns_Index_Page()

        {
            var result = controller.Index() as ViewResult;

            Assert.IsNotNull(result);

        }

        [Test]
        public void Test_Index_Search()
        {

            var result = controller.Index("01/011/1001", 1, 50) as ViewResult;

            Assert.AreEqual(1, ((PagedList<Holding>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_PacksReq_Returns_PacksReq()
        {
            var result = controller.PacksRequested() as ViewResult;

            Assert.AreEqual(1, ((List<PackRequested>)result.ViewData.Model).Count);

        }

        [Test]

        public void Test_GeneratedPacks_Returns_GeneratedPacks()
        {
            var result = controller._GeneratedPacks() as PartialViewResult;

            Assert.AreEqual(3, ((List<PackRequested>)result.ViewData.Model).Count);

        }


        [Test]

        public void Test_ViewHolding_Displays_Holding()
        {

            var result = controller.ViewHolding(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2")) as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_ViewLinkedHoldings_Displays_LinkedHolding()
        {
            var result = controller._ViewLinkedHoldings(Guid.Parse("D0F53C5B-9717-48B1-A40F-0044003DDC14")) as PartialViewResult;

            Assert.AreEqual(1, ((List<LinkedHolding>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_ViewCattle_Displays_Cattle()
        {
            var result = controller._ViewCattle(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70")) as PartialViewResult;

            Assert.AreEqual(4, ((List<Cattle>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_LateCattle_Displays_Cattle()
        {
            var result = controller._LateCattle(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70")) as PartialViewResult;

            Assert.AreEqual(2, ((List<Cattle>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_CountCattle_Displays_Cattle()
        {
            var result = controller._CountCattle(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70")) as PartialViewResult;

            Assert.AreEqual(4, ((List<Cattle>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_ViewPackRequested_ViewsPack()
        {
            var result = controller._ViewPackRequested(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2")) as PartialViewResult;

            Assert.AreEqual(true, ((PackRequested)result.ViewData.Model).activeRequest);
        }

        [Test]

        public void Test_PackRequest_Posts()
        {
            var result = controller.PackRequest(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2"), "12/122/1222");

            context.MockContext.Verify(x => x.SaveChanges(), Times.Exactly(1));
        }

        [Test]

        public void Test_PeopleData_UpdatesPeopleData()
        {
            var result = controller.PeopleUpdate(Guid.Parse("4BA355C8-920D-420D-8E80-3664A0816458"), "Scott2@test.com");

            context.MockContext.Verify(x => x.SaveChanges(), Times.Exactly(1));

        }

        [Test]

        public void Test_DeletePack_DeletesPack()
        {
            var result = controller.DeletePack("TestPath", Guid.Parse("0C51DC76-B92D-420A-BA5F-006E564C06ED")) as RedirectToRouteResult;

            Assert.AreEqual("PacksRequested", result.RouteValues["Action"]);
            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }

        [Test]

        public void Test_GetPack_getsPack()
        {
            var result = controller.GetPack("test");

            Assert.AreEqual("test", ((FileContentResult)result).FileDownloadName);
            Assert.AreEqual("application/octet-stream", ((FileContentResult)result).ContentType);
        }
    }
}
