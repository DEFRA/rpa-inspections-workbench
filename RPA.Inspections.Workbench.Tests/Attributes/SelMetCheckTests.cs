using NUnit.Framework;
using RPA.Inspections.Workbench.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.Attributes
{
    public class SelMetCheckTests
    {
        SelMetCheck selMetAttribute;

        [SetUp]

        public void Setup()
        {
            selMetAttribute = new SelMetCheck();
        }

        [Test]
        public void Test_SelMet_Accepts_Valid_SelMet()
        {
            var result = selMetAttribute.IsValid("Test");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_SelMet_Rejects_Blank_String()
        {
            var result = selMetAttribute.IsValid("");
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_SelMet_Rejects_null()
        {
            var result = selMetAttribute.IsValid(null);
            Assert.IsFalse(result);
        }




    }
}
