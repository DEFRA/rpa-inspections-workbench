using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Factory;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.SL
{   
    [TestFixture]
    [Category("Holding Service")]
    public class HoldingServiceTests
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;

        IFileService fileService;
        IHoldingService holdingService;
        IFilterService filterService;
        IUserHelper userHelper;

        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            filterService = new FilterService(context.MockContext.Object, peopleContext.MockContext.Object);
            userHelper = new UserHelper(peopleContext.MockContext.Object);
            holdingService = new HoldingService(context.MockContext.Object, peopleContext.MockContext.Object, filterService, userHelper);


            HttpContextManager.SetCurrentContext(MockHttpContext.GetMockedHttpContext());
        }

        [Test]
        public void Test_GetHolding_GetsHoldingsByCPHSearch()
        {

            //Arrange
            //Already Done in Setup

            //Act

            var result = holdingService.GetHoldingByCPH("01/011/1001");

            //Assert

            Assert.AreEqual("Big Farm", result.primaryName);

        }


        [Test]
        public void Test_GetAllHoldings_GetsAllHoldings()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = holdingService.GetAllHolding("01/011/1001", 1, 50);

            //Assert

            Assert.IsNotNull(result);
        }

        [Test]
        public void Test_List_GetsAll_with_SearchString()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = holdingService.PersonList("M600300");

            //Assert

            Assert.AreEqual(1, result.Count);

        }


        [Test]
        public void Test_List_GetsAll_without_SearchString()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = holdingService.PersonList("");

            //Assert

            Assert.AreEqual(1, result.Count);

        }

        [Test]
        public void Test_GetAllArchivedHoldings_GetsAllArchivedHoldings_ordersbyPrimaryName()
        {
            //Arrange
            //Already Done in Setup
            List<string> archivedYears = new List<string>
            {
                "2016",
                "2017",
                "2018"
            };
            //Act
            var result = holdingService.GetAllArchivedHoldings("", archivedYears, "");

            //Assert
            Assert.AreEqual("Big Farm3", result[0].primaryName);

        }

        [Test]

        public void Test_GetHoldingById_GetsHolding()
        {

            //Arrange
            //Already Done in Setup


            //Act
            var result = holdingService.GetHoldingById(Guid.Parse("214EC889-7EC8-46AD-AFA8-0030607A34EF"));


            //Assert
            Assert.AreEqual("01/011/1002", result.cphNumber);


        }

        [Test]
        public void Test_GetArchivedHoldingById_GetsArchivedHolding()
        {

            //Arrange
            //Already Done in Setup


            //Act
            var result = holdingService.GetArchivedHoldingById(Guid.Parse("214EC889-7EC8-46AD-AFA8-0030607A34EF"));


            //Assert
            Assert.AreEqual("01/011/1002", result.cphNumber);

        }






    }
}
