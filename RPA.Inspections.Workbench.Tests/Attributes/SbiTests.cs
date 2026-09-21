using NUnit.Framework;
using RPA.Inspections.Workbench.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Attributes
{
    public class SbiTests
    {
        SBIcheck sbiAttribute;

        [SetUp]

        public void Setup()
        {
            sbiAttribute = new SBIcheck();
        }

        [Test]
        public void Test_SBI_Accepts_Valid_SBI()
        {
            var result = sbiAttribute.IsValid("105100100");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SBI_Accepts_0()
        {
            var result = sbiAttribute.IsValid(0);
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SBI_Accepts_9_digits()
        {
            var result = sbiAttribute.IsValid(123456789);
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SBI_Accepts_null()
        {
            var result = sbiAttribute.IsValid(null);
            Assert.IsTrue(result);
        }


        [Test]
        public void Test_SBI_Rejects_Too_High()
        {
            var result = sbiAttribute.IsValid("1001001001");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_SBI_Rejects_Not_A_Number()
        {
            var result = sbiAttribute.IsValid("Invalid");
            Assert.IsFalse(result);
        }
    }
}
