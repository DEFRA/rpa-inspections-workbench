using NUnit.Framework;
using RPA.Inspections.Workbench.Services.SheetModels.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Services.Functions
{
    [TestFixture]
    [Category("Ear Tag Functions")]
    public class EarTagTests
    {

        [Test]

        public void Test_EarTagBreakdown_handles_historic_tags()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("UK200968700548");

            //Assert

            Assert.AreEqual(result[0], "UK200968");
            Assert.AreEqual(result[1], "7");
            Assert.AreEqual(result[2], "00548");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_historic_tags_with_spaces()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("UK V3458 00084");

            //Assert

            Assert.AreEqual(result[0], "UK V3458");
            Assert.AreEqual(result[1], " ");
            Assert.AreEqual(result[2], "00084");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_historic_tags_with_oo()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("UK2009687005oo");

            //Assert

            Assert.AreEqual(result[0], "");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_new_BEID_14digits_GB()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("GB070341270001");

            //Assert

            Assert.AreEqual(result[0], "703412");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "70001");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_new_BEID_14digits_GB_underscore()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("GB_70341270001");

            //Assert

            Assert.AreEqual(result[0], "703412");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "70001");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_new_BEID_14digits_GB_spaces()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("GB 70341270001");

            //Assert

            Assert.AreEqual(result[0], "703412");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "70001");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_new_BEID_15digits_826()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("826070341270001");

            //Assert

            Assert.AreEqual(result[0], "703412");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "70001");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_new_BEID_15digits_826_underscores()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("826_70341270001");

            //Assert

            Assert.AreEqual(result[0], "703412");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "70001");
        }

        [Test]

        public void Test_EarTagBreakdown_handles_new_BEID_15digits_826_spaces()
        {
            //Arrange


            //Act

            var result = EarTag.EarTagBreakdown("826 70341270001");

            //Assert

            Assert.AreEqual(result[0], "703412");
            Assert.AreEqual(result[1], "");
            Assert.AreEqual(result[2], "70001");
        }
    }
}
