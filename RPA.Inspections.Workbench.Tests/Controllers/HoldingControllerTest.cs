using Moq;
using NUnit.Framework;
using X.PagedList;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.ViewComponents;
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
using Microsoft.AspNetCore.Mvc;

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

            userHelper = new UserHelper(peopleContext.MockContext.Object, MockHttpContext.GetMockedHttpContextAccessor());
            filterService = new FilterService(context.MockContext.Object, peopleContext.MockContext.Object);
            holdingService = new HoldingService(context.MockContext.Object, peopleContext.MockContext.Object, filterService, userHelper);
            linkedHoldingService = new LinkedHoldingService(context.MockContext.Object);
            cattleService = new CattleService(context.MockContext.Object);
            sendService = new Mock<ISendService>();
            fileService = new Mock<IFileService>();
            mailService = new Mock<IMailService>();
            holidayService = new Mock<IHolidayService>();
            packRequestedService = new PackRequestedService(context.MockContext.Object, mailService.Object, userHelper, sendService.Object);

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
            var result = new GeneratedPacksViewComponent(packRequestedService).WithViewContext().Invoke();

            Assert.AreEqual(3, result.ModelOf<List<PackRequested>>().Count);

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
            var result = new ViewLinkedHoldingsViewComponent(linkedHoldingService).WithViewContext().Invoke(Guid.Parse("D0F53C5B-9717-48B1-A40F-0044003DDC14"));

            Assert.AreEqual(1, result.ModelOf<List<LinkedHolding>>().Count);
        }

        [Test]

        public void Test_ViewCattle_Displays_Cattle()
        {
            var result = new ViewCattleViewComponent(cattleService).WithViewContext().Invoke(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            Assert.AreEqual(4, result.ModelOf<List<Cattle>>().Count);
        }

        [Test]

        public void Test_LateCattle_Displays_Cattle()
        {
            var result = new LateCattleViewComponent(cattleService).WithViewContext().Invoke(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            Assert.AreEqual(2, result.ModelOf<List<Cattle>>().Count);
        }

        [Test]

        public void Test_CountCattle_Displays_Cattle()
        {
            var result = new CountCattleViewComponent(cattleService).WithViewContext().Invoke(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            Assert.AreEqual(4, result.ModelOf<List<Cattle>>().Count);
        }

        [Test]

        public void Test_ViewPackRequested_ViewsPack()
        {
            var result = new ViewPackRequestedViewComponent(packRequestedService).WithViewContext().Invoke(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2"));

            Assert.AreEqual(true, result.ModelOf<PackRequested>().activeRequest);
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
            var result = controller.DeletePack("TestPath", Guid.Parse("0C51DC76-B92D-420A-BA5F-006E564C06ED")) as RedirectToActionResult;

            Assert.AreEqual("PacksRequested", result.ActionName);
            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }

        [Test]

        public void Test_GetPack_getsPack()
        {
            var result = controller.GetPack("test");

            Assert.AreEqual("test", ((FileContentResult)result).FileDownloadName);
            Assert.AreEqual("application/octet-stream", ((FileContentResult)result).ContentType);
        }

        [Test]
        public void Test_GetPack_Rejects_Paths_That_Are_Not_Generated_Packs()
        {
            Assert.IsInstanceOf<NotFoundResult>(controller.GetPack("C:\\Windows\\win.ini"));
        }

        [Test]
        public void Test_DeletePack_Rejects_Paths_That_Do_Not_Belong_To_The_Pack()
        {
            Assert.IsInstanceOf<NotFoundResult>(controller.DeletePack("C:\\Windows\\win.ini", Guid.Parse("0C51DC76-B92D-420A-BA5F-006E564C06ED")));
        }
    }
}
