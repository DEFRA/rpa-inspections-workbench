using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Tests.Controllers
{
    [TestFixture]
    class BcmsAdminControllerTest
    {
        MockWorkbenchContext context;
        BcmsAdminController controller;

        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
           
            controller = new BcmsAdminController(context.MockContext.Object);
        }

        [Test]
        public void Index()
        {
            // Arrange

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Test_BulkRequest()
        {
            DateTime startDate = new DateTime(2019,01,01);
            DateTime endDate = new DateTime(2019,01,02);

            var result = controller.BulkRequest(startDate, endDate) as ViewResult;

            context.MockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [Test]
        public void Test_SpecificRequest()
        {
            string cph = "01/234/5678";
            int schemeYear = 2018;

            var result = controller.SpecificRequest(cph, schemeYear) as ViewResult;

            context.MockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [Test]
        public void Test_OpenReport_Returns_Value()
        {

            var result = controller.open_report("test");

            Assert.AreEqual("test", ((FilePathResult)result).FileName);
            Assert.AreEqual("text/html", ((FilePathResult)result).ContentType);
        }
    }
}