
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
using System.Web.Mvc;

namespace RPA.Inspections.Workbench.Tests.SL
{
    [TestFixture]
    [Category("Cattle Service")]
    public class CattleServiceTests
    {
        MockWorkbenchContext context;

        ICattleService cattleService;

        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            cattleService = new CattleService(context.MockContext.Object);
        }

        [Test]
        public void Test_GetCattleByHoldingId_Returns_Cattle()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = cattleService.GetCattleByHoldingId(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            //Assert

            Assert.AreEqual(4, result.Count());

        }

        [Test]
        public void Test_GetCattleByCPH_ReturnsCattle()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = cattleService.GetCattleByCPH("01/011/1003");

            //Assert

            Assert.AreEqual(8, result.Count());


        }

        [Test]

        public void Test_GetCattleByHoldingId_ReturnsList2DeadWithinMonth()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = cattleService.GetCattleByHoldingId(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            //Assert

            Assert.AreEqual(2, result.Where(x => x.listNumber ==("LIST2")).Count());


        }


        [Test]
        public void Test_GetLateCattle_GetsLateCattle()
        {

            //Arrange
            //Already Done in Setup

            //Act
            var result = cattleService.GetLateCattle(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70"));

            //Assert

            Assert.AreEqual(2, result.Count());

        }


    }
}
