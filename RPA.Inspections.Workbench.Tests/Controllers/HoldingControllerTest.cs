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

            userHelper = new UserHelper(peopleContext.Context, MockHttpContext.GetMockedHttpContextAccessor());
            filterService = new FilterService(context.Context, peopleContext.Context);
            holdingService = new HoldingService(context.Context, peopleContext.Context, filterService, userHelper);
            linkedHoldingService = new LinkedHoldingService(context.Context);
            cattleService = new CattleService(context.Context);
            sendService = new Mock<ISendService>();
            fileService = new Mock<IFileService>();
            mailService = new Mock<IMailService>();
            holidayService = new Mock<IHolidayService>();
            packRequestedService = new PackRequestedService(context.Context, mailService.Object, userHelper, sendService.Object, Mock.Of<IFileStorage>());
            fileService.Setup(x => x.StreamFile(It.IsAny<string>())).Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            controller = new HoldingController(context.Context, peopleContext.Context, packRequestedService, fileService.Object, userHelper);

        }

        [Test]

        public void Test_GeneratedPacks_Returns_GeneratedPacks()
        {
            var result = new GeneratedPacksViewComponent(packRequestedService).WithViewContext().Invoke();

            Assert.AreEqual(3, result.ModelOf<List<PackRequested>>().Count);

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
            controller.PackRequest(Guid.Parse("9548D01E-15FC-4CB5-97AB-000EB6EFD6C3"), "12/122/1222");
            Assert.That(context.NewContext().PackRequested.Count(), Is.EqualTo(5));

            
        }

        [Test]

        public void Test_PeopleData_UpdatesPeopleData()
        {
            var result = controller.PeopleUpdate(Guid.Parse("4BA355C8-920D-420D-8E80-3664A0816458"), "Scott2@test.com");

            

        }

        [Test]

        public void Test_GetPack_getsPack()
        {
            var result = controller.GetPack("test");

            using var stream = ((FileStreamResult)result).FileStream;
            Assert.AreEqual("test", ((FileStreamResult)result).FileDownloadName);
            Assert.AreEqual("application/octet-stream", ((FileStreamResult)result).ContentType);
        }

        [Test]
        public void Test_GetPack_Rejects_Paths_That_Are_Not_Generated_Packs()
        {
            Assert.IsInstanceOf<NotFoundResult>(controller.GetPack("C:\\Windows\\win.ini"));
            fileService.Verify(value => value.StreamFile(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void MissingBlobForRecordedPackReturnsNotFound()
        {
            fileService.Setup(value => value.StreamFile("test")).Throws(new FileNotFoundException());
            Assert.That(controller.GetPack("test"), Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void BlobPackDownloadPreservesFriendlyFilename()
        {
            var record = context.Context.PackRequested.First(x => x.savePath == "test");
            record.savePath = "packs/request/generated/CII Animal Data Spreadsheet123456789.xlsm";
            context.Context.SaveChanges();
            var result = (FileStreamResult)controller.GetPack(record.savePath);
            using var stream = result.FileStream;
            Assert.That(result.FileDownloadName, Is.EqualTo("CII Animal Data Spreadsheet123456789.xlsm"));
        }

    }
}




