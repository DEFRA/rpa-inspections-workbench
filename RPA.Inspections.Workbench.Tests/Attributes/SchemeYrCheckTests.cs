using NUnit.Framework;
using RPA.Inspections.Workbench.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Attributes
{
    public class SchemeYrCheckTests
    {
        SchemeYrcheck schemeYrcheckAttribute;

        [SetUp]

        public void Setup()
        {
            schemeYrcheckAttribute = new SchemeYrcheck();
        }

        [Test]
        public void Test_SchemeYr_Accepts_Valid_SchemeYr()
        {
            var result = schemeYrcheckAttribute.IsValid("2020");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SchemeYr_Rejects_Invalid_SchemeYr()
        {
            var result = schemeYrcheckAttribute.IsValid("20200");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_SchemeYr_Rejects_Text()
        {
            var result = schemeYrcheckAttribute.IsValid("Hello");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_SchemeYr_Rejects_Null()
        {
            var result = schemeYrcheckAttribute.IsValid(null);
            Assert.IsFalse(result);
        }
    }
}
