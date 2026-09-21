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
using System.Web;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Tests.Controllers
{
    public class AdminControllerTest
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;
        AdminController controller;

        IHoldingService holdingService;
        IBreedService breedService;
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

            breedService = new BreedService(context.MockContext.Object);
            cattleService = new CattleService(context.MockContext.Object);
            fileService = new Mock<IFileService>();
            mailService = new Mock<IMailService>();
            sendService = new Mock<ISendService>();
            
            holidayService = new Mock<IHolidayService>();
            packRequestedService = new PackRequestedService(context.MockContext.Object, mailService.Object, userHelper, sendService.Object);

            HttpContextManager.SetCurrentContext(MockHttpContext.GetMockedHttpContext());

            controller = new AdminController(context.MockContext.Object, peopleContext.MockContext.Object, holdingService, cattleService, breedService, packRequestedService, mailService.Object, fileService.Object, filterService, userHelper, sendService.Object, holidayService.Object);

        }

        [Test]

        public void Test_Index_Returns_Index_Page()

        {
            var result = controller.Index() as ViewResult;

            Assert.IsNotNull(result);

        }

        [Test]

        public void Test_Index_Post_Returns_Index_Page()

        {
            Mock<HttpPostedFileBase> uploadedFile = new Mock<HttpPostedFileBase>();

            var result = controller.Index(uploadedFile.Object, true) as RedirectToRouteResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_Breed_List_Returns_List_of_Breeds()
        {
            var result = controller.BreedList() as ViewResult;

            Assert.AreEqual(4, ((PagedList<Breed>)result.ViewData.Model).Count);
        }

        [Test]
        public void Test_Breed_Search()
        {

            var result = controller.BreedList("Mega Cow 1", 1, 50) as ViewResult;

            Assert.AreEqual(1, ((PagedList<Breed>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_Add_Breed_Returns_Add_Breed_View()
        {
            var result = controller.AddBreed() as ViewResult;

            Assert.IsNotNull(result);

        }

        [Test]

        public void Test_View_Breed_Returns_Breed()
        {
            var result = controller.ViewBreed(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79")) as ViewResult;

            Assert.AreEqual("Mega Cow", ((Breed)result.ViewData.Model).breedFull);
        }

        [Test]

        public void Test_Holding_List_Gets_Holdings()
        {
            var result = controller.HoldingList() as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_Holding_List_Gets_Holdings_with_SearchString()
        {
            var result = controller.HoldingList("01/011/1001") as ViewResult;

            Assert.AreEqual(1, ((PagedList<Holding>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_Add_Holding_Displays_Add_Holding_Page()
        {
            var result = controller.AddHolding() as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_ViewHolding_Displays_Holding()
        {

            var result = controller.ViewHolding(Guid.Parse("9548D01E-15FC-4CB5-97AB-000EB6EFD6C3")) as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_Archived_List_Gets_Archived_List()
        {
            var result = controller.ArchivedList() as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_Archived_Holding_By_Year_Selection()
        {
            var result = controller.ArchivedList(null, "2017") as ViewResult;

            Assert.AreEqual(2, ((List<Holding>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_Archived_Holding_List_Gets_Archived_Holdings_with_SearchString()
        {
            var result = controller.ArchivedList("01/011/1005") as ViewResult;

            Assert.AreEqual(1, ((List<Holding>)result.ViewData.Model).Count);
        }

        [Test]

        public void Test_ViewArchivedHolding_Returns_Archived_Holding()
        {
            var result = controller.ViewArchivedHolding(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2")) as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_Inspector_List_Gets_Inspector_List()
        {
            var result = controller.InspectorList() as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_AddInspector_Gets_AddInspector_View()
        {
            var result = controller.AddInspector() as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_AddInspector_Adds_Inspector()
        {
            Inspector inspector = new Inspector
            {
                inspectorId = Guid.NewGuid(),
                staffNumber = "M100100"
            };

            var result = controller.AddInspector(inspector) as ViewResult;

            Assert.AreEqual(2, context.MockContext.Object.Inspector.Count());
        }

        [Test]

        public void Test_AddInspector_Does_No_Add_Inspector_if_already_exists()
        {
            Inspector inspector = new Inspector
            {
                inspectorId = Guid.NewGuid(),
                staffNumber = "M600300"
            };

            var result = controller.AddInspector(inspector) as ViewResult;

            Assert.AreEqual(1, context.MockContext.Object.Inspector.Count());
        }

        [Test]

        public void Test_RemoveInspector_Removes_Inspector()
        {

            var result = controller.RemoveInspector(Guid.Parse("C5F36F2D-C531-474C-8688-01831DCF1F0E")) as ViewResult;

            Assert.AreEqual(0, context.MockContext.Object.Inspector.Count());
        }


        [Test]

        public void Test_AddBreed_AddsBreed()
        {
            Breed breed = new Breed
            {
                breedId = new Guid(),
                breedCode = "BFY",
                breedFull = "Beefy",
                Active = true
            };

            var result = controller.AddBreed(breed) as ViewResult;

            Assert.AreEqual(5, context.MockContext.Object.Breed.Count());
        }



        [Test]

        public void Test_ViewBreed_Saves_Changes()
        {
            Breed breed = context.MockContext.Object.Breed.Where(x => x.breedId == Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79")).FirstOrDefault();

            breed.breedFull = "Mega Mega Cow";

            var result = controller.ViewBreed(breed) as ViewResult;

            Assert.AreEqual("Mega Mega Cow", context.MockContext.Object.Breed.Where(x => x.breedId == Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79")).Select(p=>p.breedFull).FirstOrDefault());

            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }


        

        [Test]

        public void Test_AddHolding_AddsHolding()
        {

            Holding holding = new Holding
            {
                holdingId = Guid.NewGuid(),
                Active = true,
                animalsToBeInspected = 50,
                cphNumber = "88/888/8888",
                AssignedUser = "M600300",
                inspectionDeadlineDate = DateTime.Now,
                SBI = 123456789,
                SchemeNameDropId = 1,
                schemeYear = "2020"
                
            };

            var result = controller.AddHolding(holding) as ViewResult;

            Assert.AreEqual(7, context.MockContext.Object.Holding.Count());

        }

        [Test]
        public void Test_ViewHolding_Edits_Holding()
        {
            Holding holding = context.MockContext.Object.Holding.Where(x => x.holdingId == Guid.Parse("9548D01E-15FC-4CB5-97AB-000EB6EFD6C3")).FirstOrDefault();

            holding.primaryName = "Potato Farm";

            var result = controller.ViewHolding(holding) as ViewResult;

            Assert.AreEqual("Potato Farm", context.MockContext.Object.Holding.Where(x => x.holdingId == Guid.Parse("9548D01E-15FC-4CB5-97AB-000EB6EFD6C3")).Select(p => p.primaryName).FirstOrDefault());

            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }

        [Test]

        public void Test_SchemeYearAdmin_Returns_View()
        {

            var result = controller.SchemeYearAdmin() as ViewResult;

            Assert.IsNotNull(result);
        }

        [Test]

        public void Test_SchemeYearAdmin_Edits_on_Post()
        {

            List<Control> controlList = context.MockContext.Object.Control.ToList();

            controlList.Where(x => x.Value == "2019").First().Active = false;

            var result = controller.SchemeYearAdmin(controlList) as ViewResult;

            Assert.AreEqual(false, context.MockContext.Object.Control.Where(x => x.Value == "2019").First().Active);
            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());

        }

        [Test]

        public void Test_Add_SchemeYear_Adds_Scheme_Year()
        {

            var result = controller.AddSchemeYear() as ViewResult;

            Assert.AreEqual(4, context.MockContext.Object.Control.Count());
            Assert.AreEqual(4, context.MockContext.Object.SchemeNameDrop.Count());
            context.MockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce());
        }

        [Test]
        public void Test_PeopleSearch_Returns_Person_By_Name()
        {
            JsonResult result = controller.PeopleSearch("Gordon, [REDACTED_NAME]") as JsonResult;

            var actual = ((IEnumerable<dynamic>)result.Data).ToList();

            Assert.AreEqual("{ label = Gordon, [REDACTED_NAME], value = [REDACTED_PASSPORT] }", actual[0].ToString());
        }

        [Test]
        public void Test_PeopleSearch_Returns_Person_By_Staff_Number()
        {
            JsonResult result = controller.PeopleSearch("M600300") as JsonResult;

            var actual = ((IEnumerable<dynamic>)result.Data).ToList();

            Assert.AreEqual("{ label = Gordon, [REDACTED_NAME], value = [REDACTED_PASSPORT] }", actual[0].ToString());
        }

        //[Test]

        //public void Test_Index_Post()
        //{
        //    Mock<HttpPostedFileBase> uploadedFile = new Mock<HttpPostedFileBase>();

        //    var result = controller.Index(uploadedFile, false) as ViewResult;
        //}




    }
}
