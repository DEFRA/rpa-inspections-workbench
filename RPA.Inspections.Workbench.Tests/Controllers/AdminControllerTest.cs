using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;

namespace RPA.Inspections.Workbench.Tests.Controllers
{
    public class AdminControllerTest
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;
        AdminController controller;

        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            var userHelper = new UserHelper(peopleContext.Context, MockHttpContext.GetMockedHttpContextAccessor());
            var filterService = new FilterService(context.Context, peopleContext.Context);
            controller = new AdminController(new HoldingService(context.Context, peopleContext.Context, filterService, userHelper));
        }

        [TearDown]
        public void TearDown()
        {
            context.Dispose();
            peopleContext.Dispose();
        }

        [Test]
        public void Test_PeopleSearch_Returns_Person_By_Name()
        {
            JsonResult result = controller.PeopleSearch("Gordon, [REDACTED_NAME]") as JsonResult;

            var actual = ((IEnumerable<dynamic>)result.Value).ToList();

            Assert.AreEqual("{ label = Gordon, [REDACTED_NAME], value = M600300 }", actual[0].ToString());
        }

        [Test]
        public void Test_PeopleSearch_Returns_Person_By_Staff_Number()
        {
            JsonResult result = controller.PeopleSearch("M600300") as JsonResult;

            var actual = ((IEnumerable<dynamic>)result.Value).ToList();

            Assert.AreEqual("{ label = Gordon, [REDACTED_NAME], value = M600300 }", actual[0].ToString());
        }

            }
}

