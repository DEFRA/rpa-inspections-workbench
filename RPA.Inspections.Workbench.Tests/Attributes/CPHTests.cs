using NUnit.Framework;
using RPA.Inspections.Workbench.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Attributes
{
    public class CPHTests
    {
        CPHcheck cphAttribute;

        [SetUp]

        public void Setup()
        {
            cphAttribute = new CPHcheck();
        }

        [Test]
        public void Test_CPH_Accepts_Valid_CPH()
        {
            var result = cphAttribute.IsValid("10/100/1000");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_CPH_Rejects_Invalid_Length_CPH_Long()
        {
            var result = cphAttribute.IsValid("15/150/10005");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_CPH_Rejects_Invalid_Length_CPH_Short()
        {
            var result = cphAttribute.IsValid("15/150/105");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_CPH_Rejects_Invalid_CPH_Characters()
        {
            var result = cphAttribute.IsValid("15/HEP/105");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_CPH_Rejects_Invalid_No_Slashes()
        {
            var result = cphAttribute.IsValid("15150105");
            Assert.IsFalse(result);
        }
    }
}

