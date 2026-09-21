using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services.SheetModels;
using RPA.Inspections.Workbench.SL;
using System;

namespace RPA.Inspections.Workbench.Tests.Services.CattleFunctions
{
    [TestFixture]
    [Category("CattleFunctions")]
    public class LateCattleCalculatorTests
    {
        LateCattleCalculator lateCattleCalculator;
        LateCattleCount lateCattleCount;
        Mock<IHolidayService> holidayService;


        [SetUp]

        public void Setup()
        {
            holidayService = new Mock<IHolidayService>();
            lateCattleCalculator = new LateCattleCalculator(holidayService.Object);
            lateCattleCount = new LateCattleCount();
        }

        [Test]

        public void Test_Calculator_Returns_FMXC1_LATEON()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 3, 21),
                lateType = "LATE_ON"
            };

            holidayService.Setup(x => x.GetWorkingDays(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(8);

            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.FMXC1Count, 1);
        }

        [Test]

        public void Test_Calculator_Returns_FMXC2_LATEON()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 5, 21),
                lateType = "LATE_ON"
            };

            holidayService.Setup(x => x.GetWorkingDays(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(22);

            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.FMXC2Count, 1);
        }

        [Test]

        public void Test_Calculator_Returns_FMXC1_LATEOFF()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 3, 21),
                lateType = "LATE_OFF"
            };

            holidayService.Setup(x => x.GetWorkingDays(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(8);

            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.FMXC1Count, 1);
        }

        [Test]
        public void Test_Calculator_Returns_FMXC2_LATEOFF()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 5, 21),
                lateType = "LATE_OFF"
            };

            holidayService.Setup(x => x.GetWorkingDays(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(22);

            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.FMXC2Count, 1);
        }

        [Test]

        public void Test_Calculator_Returns_DMXC1_LATEDEATH()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 3, 21),
                lateType = "LATE_DEATH"
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.DDXC1Count, 1);
        }

        [Test]
        public void Test_Calculator_Returns_DMXC2_LATEDEATH()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 5, 21),
                lateType = "LATE_DEATH"
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.DDXC2Count, 1);
        }

        [Test]

        public void Test_Calculator_Returns_LRXC1_LATEBIRTH()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 4, 10),
                lateType = "LATE_BIRTH"
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.LRXC1Count, 1);
        }

        [Test]
        public void Test_Calculator_Returns_LRXC2_LATEBIRTH()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 10, 21),
                lateType = "LATE_BIRTH"
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.LRXC2Count, 1);
        }

        [Test]

        public void Test_Calculator_Returns_IMPXC1_LATEIMPORT()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 3, 25),
                lateType = "LATE_IMPORT"
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.IMPXC1Count, 1);
        }

        [Test]
        public void Test_Calculator_Returns_IMPXC2_LATEIMPORT()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 5, 21),
                lateType = "LATE_IMPORT"
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.IMPXC2Count, 1);
        }

        [Test]
        public void Test_Calculator_Returns_Default()
        {
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                lateDate = new DateTime(2021, 3, 6),
                notificationDate = new DateTime(2021, 5, 21),
                lateType = ""
            };


            // Act

            var result = lateCattleCalculator.Calculator(cattle, lateCattleCount, holidayService.Object);

            //Assert

            Assert.AreEqual(result.FMXC1Count, 0); 
            Assert.AreEqual(result.FMXC2Count, 0);
            Assert.AreEqual(result.DDXC1Count, 0);
            Assert.AreEqual(result.DDXC2Count, 0);
            Assert.AreEqual(result.LRXC1Count, 0);
            Assert.AreEqual(result.LRXC2Count, 0);
            Assert.AreEqual(result.IMPXC2Count, 0);
            Assert.AreEqual(result.IMPXC2Count, 0);
        }
    }
}
