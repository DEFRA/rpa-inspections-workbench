
using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.SL
{
    [TestFixture]
    [Category("Pack Requested Service")]
    public class PackRequestedServiceTests
    {

        MockWorkbenchContext context;
        MockPeopleContext peopleContext;

        IPackRequested packService;
        Mock<IMailService> mailService;
        IUserHelper userHelper;
        Mock<ISendService> sendService;



        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            mailService = new Mock<IMailService>();
            sendService = new Mock<ISendService>();
            userHelper = new UserHelper(peopleContext.MockContext.Object);

            packService = new PackRequestedService(context.MockContext.Object, mailService.Object, userHelper, sendService.Object);
        }

        [Test]

        public void Test_GetPackRequestedById_getsPacksRequested()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = packService.GetPackRequestedById(Guid.Parse("A6EFE560-FE03-40A2-94CF-00368986EDE2"));


            //Assert

            Assert.AreEqual("m600300", result.requestedBy);

        }


        [Test]

        public void Test_GetPackRequestedById_only_gets_active_packs()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = packService.GetPackRequestedById(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));


            //Assert

            Assert.AreEqual(null, result);

        }

        [Test]
        //This will also test the email send method
        public void Test_CreatePackRequested_CreatesPack()
        {
                                  

            //Act

            packService.CreatePackRequested(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"), "10/100/1000");

            //Assert

            var result = packService.GetPackRequestedById(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            Assert.IsNotNull(result);

        }

       

        [Test]
        public void Test_GetOutstandingPackRequests_GetsOutstandingPackRequests()
        {

            //Arrange
            //Already Done in Setup

        
            //Act

            var result = packService.GetOutstandingPackRequests();

            //Assert

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void Test_GetDownloadableRequests_GetsDownloadablePackRequests()
        {
            //Arrange
            //done above

            //Act

            var result = packService.GetDownloadableRequests();

            //Assert

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void Test_DeletePack_DeletesPackfromPackReqTable()
        {

            ////Arrange
            //Done Above


            //Act

            packService.DeletePack("Test", Guid.Parse("A4609495-1FA5-4BEB-9B09-00777D219F31"));

            //Assert

            var result = context.MockPackRequested.Object.Where(x => x.packRequestedId == Guid.Parse("A4609495-1FA5-4BEB-9B09-00777D219F31")).FirstOrDefault();

            Assert.IsFalse(result.display);
        }


    }
}
