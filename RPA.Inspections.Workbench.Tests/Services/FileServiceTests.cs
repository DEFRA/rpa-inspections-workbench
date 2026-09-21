using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Factory;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.Services.Functions;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.SL
{
    [TestFixture]
    [Category("File Service")]
    public class FileServiceTests
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;

        IFileService fileService;
        ICattleService cattleService;
        Mock<IMailService> mailService;
        IHoldingService holdingService;
        IFilterService filterService;
        IUserHelper userHelper;
        Mock<ISendService> sendService;
        Mock<IHolidayService> holidayService;


        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            cattleService = new CattleService(context.MockContext.Object);
            sendService = new Mock<ISendService>();
            holidayService = new Mock<IHolidayService>();
            mailService = new Mock<IMailService>();
            filterService = new FilterService(context.MockContext.Object, peopleContext.MockContext.Object);
            userHelper = new UserHelper(peopleContext.MockContext.Object);
            holdingService = new HoldingService(context.MockContext.Object, peopleContext.MockContext.Object, filterService, userHelper);
            fileService = new FileService(context.MockContext.Object, cattleService, holdingService, sendService.Object, mailService.Object, holidayService.Object);

            HttpContextManager.SetCurrentContext(MockHttpContext.GetMockedHttpContext());
        }

        [Test]

        public void Test_AddToHolding_AddsHolding()
        {
            //Arrange

            DataTable fakeHolding = new DataTable();

            for (int i = 0; i < 9; i++)
            {
                fakeHolding.Columns.Add();
            }

            DataRow newrow = fakeHolding.NewRow();

            newrow[0] = "01/011/0111";
            newrow[1] = "123456789";
            newrow[2] = "2020";
            newrow[3] = "CII";
            newrow[4] = "RISK";
            newrow[5] = "M600300";
            newrow[6] = "50";
            newrow[7] = DateTime.Now;


            //Act

            fileService.AddToHolding(newrow);

            //Assert

            Assert.AreEqual(7, context.MockContext.Object.Holding.Count());
            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }

        [Test]

        public void Test_AddToHolding_AddsHolding_handles_empty_values()
        {
            //Arrange

            DataTable fakeHolding = new DataTable();

            for (int i = 0; i < 9; i++)
            {
                fakeHolding.Columns.Add();
            }

            DataRow newrow = fakeHolding.NewRow();

            newrow[0] = "01/011/0111";
            newrow[1] = "123456789";
            newrow[2] = "";
            newrow[3] = "EGG";
            newrow[4] = "";
            newrow[5] = "M600300";
            newrow[6] = "50";
            newrow[7] = DateTime.Now;


            //Act

            fileService.AddToHolding(newrow);

            //Assert

            Assert.AreEqual(7, context.MockContext.Object.Holding.Count());
            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }

        [Test]

        public void Test_UpdatePackReq_UpdatesPackReq()
        {
            //Arrange


            Holding holding = context.MockContext.Object.Holding.Where(x => x.holdingId == Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70")).First();
            PackRequested packRequested = context.MockContext.Object.PackRequested.Where(x => x.packRequestedId == Guid.Parse("A4609495-1FA5-4BEB-9B09-00777D219F31")).First();


            //Act

            fileService.UpdatePackReq(holding, packRequested, "TestPath");

            //Assert

            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }



        [Test]
        public void TestClearTables_ClearsTables()
        {

            //Arrange


            //Act

            fileService.ClearTables();

            //Assert

            Assert.AreEqual(0, context.MockContext.Object.PackRequested.Count());
            Assert.AreEqual(0, context.MockContext.Object.Cattle.Count());
            Assert.AreEqual(0, context.MockContext.Object.LinkedHolding.Count());
            Assert.AreEqual(2, context.MockContext.Object.Holding.Count());
        }


       [Test]

        public void Test_safe_CPH_builder_returns_safe_CPH()
        {

            //Arrange
            string dodgyCPH = "30  /123/  1234";

            //Act

            var result = fileService.SafeCPHBuilder(dodgyCPH);

            //Assert

            Assert.AreEqual("301231234", result);


        }

        [Test]

        public void Test_PathBuilder_Returns_Path()
        {
            //Arrange

            //Act

            var result = fileService.PathBuilder("Test");

            //Assert


            Assert.AreEqual((AppDomain.CurrentDomain.BaseDirectory + "Uploads\\Test"), result);

        }

        [Test]

        public void Test_TenPercentCattleSelector_Selects_Correctly_Option_1()
        {
            //Arrange

            List<Cattle> cattleList = context.MockCattle.Object.ToList();

            //Act

            var result = TenPercentCattleSelector.CattleSelector(1, cattleList, 10);

            //Assert

            Assert.AreEqual(1, result.Count);

        }

        [Test]

        public void Test_TenPercentCattleSelector_Selects_Correctly_Option_2()
        {
            //Arrange

            List<Cattle> cattleList = context.MockCattle.Object.ToList();

            //Act

            var result = TenPercentCattleSelector.CattleSelector(2, cattleList, 10);

            //Assert

            Assert.AreEqual(1, result.Count);

        }

        [Test]

        public void Test_TenPercentCattleSelector_Selects_Correctly_Option_3()
        {
            //Arrange

            List<Cattle> cattleList = context.MockCattle.Object.ToList();

            //Act

            var result = TenPercentCattleSelector.CattleSelector(3, cattleList, 10);

            //Assert

            Assert.AreEqual(2, result.Count);

        }



    }
}

