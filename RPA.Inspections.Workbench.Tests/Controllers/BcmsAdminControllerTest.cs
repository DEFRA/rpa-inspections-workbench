using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.Controllers;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RPA.Inspections.Workbench.Tests.Controllers
{
    [TestFixture]
    class BcmsAdminControllerTest
    {
        MockWorkbenchContext context;
        BcmsAdminController controller;
        Mock<IFileStorage> fileStorage;

        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
           
            fileStorage = new Mock<IFileStorage>();
            fileStorage.Setup(x => x.Download(It.IsAny<string>())).Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));
            controller = new BcmsAdminController(context.MockContext.Object, fileStorage.Object);
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

            var result = controller.open_report("test1");

            using var stream = ((FileStreamResult)result).FileStream;
            fileStorage.Verify(x => x.Download("test1"), Times.Once);
            Assert.AreEqual("text/html", ((FileStreamResult)result).ContentType);
        }

        [Test]
        public void Test_OpenReport_Rejects_Paths_That_Are_Not_Bcms_Reports()
        {
            var result = controller.open_report("C:\\Windows\\win.ini");

            Assert.IsInstanceOf<NotFoundResult>(result);
            fileStorage.Verify(value => value.Download(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void MissingBlobForRecordedReportReturnsNotFound()
        {
            fileStorage.Setup(value => value.Download("test1")).Throws(new FileNotFoundException());
            Assert.That(controller.open_report("test1"), Is.TypeOf<NotFoundResult>());
        }
    }
}