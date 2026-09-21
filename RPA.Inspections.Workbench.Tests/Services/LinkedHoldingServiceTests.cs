
using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
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
    [Category("Linked Holding Service")]
    public class LinkedHoldingServiceTests
    {

        MockWorkbenchContext context;

        ILinkedHoldingService linkedHoldingService;

        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            linkedHoldingService = new LinkedHoldingService(context.MockContext.Object);
            
        }

        [Test]
        public void Test_GetLinkedHoldingById_GetsLinkedHoldings()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = linkedHoldingService.GetLinkedholdingById(Guid.Parse("D0F53C5B-9717-48B1-A40F-0044003DDC14"));

            //Assert

            Assert.AreEqual(1, result.Count);
        }

        [Test]

        public void Test_GetLinkedHoldingByCPH_ReturnsHolding()
        {
            //Arrange
            //Already Done in Setup

            //Act
            var result = linkedHoldingService.GetLinkedholdingByCPH("01/011/1004");

            //Assert

            Assert.AreEqual(1, result.Count);
        }

        [Test]

        public void Test_GetLinkedHolding_ReturnsHoldingDetailed()
        {
            //Arrange
            //Already Done in Setup



            //Act
            var result = linkedHoldingService.GetLinkedholdingById(Guid.Parse("D0F53C5B-9717-48B1-A40F-0044003DDC14"));

            //Assert

            Assert.AreEqual(1, result.Count);
        }

    }
}
