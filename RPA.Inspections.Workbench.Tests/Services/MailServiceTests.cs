using Moq;
using NUnit.Framework;
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

namespace RPA.Inspections.Workbench.Tests.Services
{
    [TestFixture]
    [Category("Mail Service")]
    public class MailServiceTests
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;


        IMailService mailService;
        Mock<ISendService> sendService;



        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            sendService = new Mock<ISendService>();
            mailService = new MailService(context.MockContext.Object);

            HttpContextManager.SetCurrentContext(MockHttpContext.GetMockedHttpContext());
        }

        [Test]

        public void Test_MailInspector_creates_email()
        {
            //Arrange

            Holding holding = context.MockHolding.Object.Where(x => x.holdingId == Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70")).First();
            PackRequested packRequested = context.MockPackRequested.Object.Where(x => x.packRequestedId == Guid.Parse("A4609495-1FA5-4BEB-9B09-00777D219F31")).First();

            //Act

            var result = mailService.GenerateInspectorEmail(holding, packRequested);

            //Assert

            Assert.AreEqual(result.Subject, "Pack Generated: 01/011/1003");
            Assert.AreEqual(result.To[0], "test@test.com");

        }

        [Test]

        public void Test_sendPackRequest_creates_email()
        {
            //Arrange

            Guid holdingId = Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2");

            //Act

            var result = mailService.sendPackRequest("01/011/1010", holdingId);

            //Assert

            Assert.AreEqual(result.Subject, "II-0-417");
            Assert.AreEqual(result.Body, "01/011/1010");

        }




    }
}
