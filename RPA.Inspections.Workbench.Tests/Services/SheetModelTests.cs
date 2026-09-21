using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services.SheetModels;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.Inspections.Workbench.Tests.SL
{
    [TestFixture]
    [Category("File Service Sheet Models")]
    public class SheetModelTests
    {
        MockWorkbenchContext context;
        Mock<IHolidayService> holidayService;

        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            holidayService = new Mock<IHolidayService>();
        }

        [Test]
        public void Test_Sheet1Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Holding holding = new Holding
            {
                cphNumber = "cphNumber",
                SBI = 1234,
                primaryName = "primaryName"
            };

            PackRequested packRequested = new PackRequested
            {
                datePackRequested = new DateTime(2021, 3, 1),
                Id = 23,
                requestedByName = "test user"
            };

            var timeNow = new DateTime(2021, 3, 2, 11, 23, 00);

            // Act
            var mock = new Mock<Sheet1Model>(timeNow, holding, packRequested, null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.Apply();

            // Assert model
            Assert.AreEqual("cphNumber", model.cphNumber);
            Assert.AreEqual(1234, model.SBI);
            Assert.AreEqual("primaryName", model.primaryName);
            Assert.AreEqual("01-03-2021 12:00 AM", model.dateRequested);
            Assert.AreEqual("CE23", model.showRef);
            Assert.AreEqual("02-03-2021 11:23 AM", model.currentTime);
            Assert.AreEqual("test user", model.requestedBy);

            // assert correct columns set
            Assert.AreEqual("cphNumber", mockSheet["E10"]);
            Assert.AreEqual(1234, mockSheet["E12"]);
            Assert.AreEqual("primaryName", mockSheet["D14"]);
            Assert.AreEqual("02-03-2021 11:23 AM", mockSheet["H1"]);
            Assert.AreEqual("CE23", mockSheet["H2"]);
            Assert.AreEqual("01-03-2021 12:00 AM", mockSheet["H3"]);
            Assert.AreEqual("test user", mockSheet["D37"]);
        }

        [Test]
        public void Test_Sheet2Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Holding holding = new Holding
            {
                cphNumber = "cphNumber",
                SBI = 1234,
                primaryName = "primaryName",
                addressLine1 = "address1",
                addressLine2 = "address2",
                addressLine3 = "address3", 
                region = "region",
                postCode = "NE1",
                telephoneNumber = "4",
                faxNumber = "5",
                mobileNumber = "6",
                schemeYear = "2020",
                selectionMethod = "random"
            };


            var timeNow = new DateTime(2021, 3, 2, 11, 23, 00);

            // Act
            var mock = new Mock<Sheet2Model>(holding, null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.Apply();

            // Assert model
            Assert.AreEqual("primaryName", model.primaryName);
            Assert.AreEqual("address1", model.addressLine1);
            Assert.AreEqual("address2", model.addressLine2);
            Assert.AreEqual("address3", model.addressLine3);
            Assert.AreEqual("region", model.region);
            Assert.AreEqual("NE1", model.postCode);
            Assert.AreEqual("4", model.telephoneNumber);
            Assert.AreEqual("5", model.faxNumber);
            Assert.AreEqual("6", model.mobileNumber);
            Assert.AreEqual(1234, model.SBI);
            Assert.AreEqual("2020", model.schemeYear);
            Assert.AreEqual("random", model.selectionMethod);

            // assert correct columns set
            Assert.AreEqual("primaryName", mockSheet["B2"]);
            Assert.AreEqual("address1", mockSheet["B3"]);
            Assert.AreEqual("address2", mockSheet["B4"]);
            Assert.AreEqual("address3", mockSheet["B5"]);
            Assert.AreEqual("region", mockSheet["B6"]);
            Assert.AreEqual("NE1", mockSheet["B8"]);
            Assert.AreEqual("4", mockSheet["B11"]);
            Assert.AreEqual("5", mockSheet["B12"]);
            Assert.AreEqual("6", mockSheet["B13"]);
            Assert.AreEqual(1234, mockSheet["B14"]);
            Assert.AreEqual("2020", mockSheet["B17"]);
            Assert.AreEqual("random", mockSheet["B18"]);
        }

        [Test]
        public void Test_Sheet3Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            LinkedHolding linkedHolding = new LinkedHolding
            {
                CPHNumberLH = "cphNumber",
                primaryNameLH = "primaryName",
                addressLine1LH = "address1",
                addressLine2LH = "address2",
                addressLine3LH = "address3",
                PostCodeLH = "NE1",
                regionLH = "X"
            };


            int rowStart = 3;

            // Act
            var mock = new Mock<Sheet3Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            CphAddress cphAddress = new CphAddress(linkedHolding);
            model.Apply(rowStart, cphAddress);

            // Assert model
            Assert.AreEqual("cphNumber", cphAddress.cphNumberLH);
            Assert.AreEqual("primaryName", cphAddress.primaryNameLH);
            Assert.AreEqual("address1", cphAddress.addressLine1LH);
            Assert.AreEqual("address2", cphAddress.addressLine2LH);
            Assert.AreEqual("address3", cphAddress.addressLine3LH);
            Assert.AreEqual("NE1", cphAddress.postCodeLH);
            Assert.AreEqual("X", cphAddress.regionLH);

            // assert correct columns set
            Assert.AreEqual("cphNumber", mockSheet["A3"]);
            Assert.AreEqual("primaryName", mockSheet["B3"]);
            Assert.AreEqual("address1", mockSheet["B4"]);
            Assert.AreEqual("address2", mockSheet["B5"]);
            Assert.AreEqual("address3", mockSheet["B6"]);
            Assert.AreEqual("NE1", mockSheet["B7"]);
            Assert.AreEqual("X", mockSheet["B8"]);
        }

        [Test]
        public void Test_Sheet3Model_ApplyLinkedHoldings_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            LinkedHolding linkedHolding = new LinkedHolding
            {
                CPHNumberLH = "cphNumber",
                primaryNameLH = "primaryName",
                addressLine1LH = "address1",
                addressLine2LH = "address2",
                addressLine3LH = "address3",
                PostCodeLH = "NE1",
                regionLH = "X",
                isLink = true
            };

            List<LinkedHolding> linkedHoldings = new List<LinkedHolding>();

            linkedHoldings.Add(linkedHolding);

            int rowStart = 3;

            // Act
            var mock = new Mock<Sheet3Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.ApplyLinkedHoldings(rowStart, linkedHoldings);

            // assert correct columns set
            Assert.AreEqual("cphNumber", mockSheet["A3"]);
            Assert.AreEqual("primaryName", mockSheet["B3"]);
            Assert.AreEqual("address1", mockSheet["B4"]);
            Assert.AreEqual("address2", mockSheet["B5"]);
            Assert.AreEqual("address3", mockSheet["B6"]);
            Assert.AreEqual("NE1", mockSheet["B7"]);
            Assert.AreEqual("X", mockSheet["B8"]);
        }

        [Test]

        public void Test_CPHAddress_returns_CPHAddress()
        {
            // Arrange
            LinkedHolding linkedHolding = new LinkedHolding
            {
                CPHNumberLH = "cphNumber",
                primaryNameLH = "primaryName",
                addressLine1LH = "address1",
                addressLine2LH = "address2",
                addressLine3LH = "address3",
                PostCodeLH = "NE1",
                regionLH = "X",
                isLink = true
            };

            CphAddress cphAddress = new CphAddress(linkedHolding);

            Assert.AreEqual("cphNumber", cphAddress.cphNumberLH);
            Assert.AreEqual("primaryName", cphAddress.primaryNameLH);
            Assert.AreEqual("address1", cphAddress.addressLine1LH);
            Assert.AreEqual("address2", cphAddress.addressLine2LH);
            Assert.AreEqual("address3", cphAddress.addressLine3LH);
            Assert.AreEqual("NE1", cphAddress.postCodeLH);
            Assert.AreEqual("X", cphAddress.regionLH);
        }

        [Test]
        public void Test_Sheet4Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            List<Cattle> cattleList = context.MockCattle.Object.ToList();


            // Act
            var mock = new Mock<Sheet4Model>(cattleList, null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.Apply();

            // Assert model
            Assert.AreEqual(2, model.listOneCount);
            Assert.AreEqual(4, model.listTwoCount);


            // assert correct columns set
            Assert.AreEqual(2, mockSheet["B3"]);
            Assert.AreEqual(4, mockSheet["B5"]);

        }

        [Test]
        public void Test_Sheet5Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull ="testBreed"},
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5"
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };

            int rowStart = 3;
            CattleDetails cattleDetails = new CattleDetails(cattle, earTagBreakdown);

            // Act
            var mock = new Mock<Sheet5Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.Apply(rowStart, cattleDetails);



            // assert correct columns set
            Assert.AreEqual("TestEarTag", mockSheet["C3"]);
            Assert.AreEqual("1", mockSheet["D3"]);
            Assert.AreEqual("2", mockSheet["E3"]);
            Assert.AreEqual("3", mockSheet["F3"]);
            Assert.AreEqual("testBreed", mockSheet["G3"]);
            Assert.AreEqual("Female", mockSheet["H3"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["I3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 2), DateTime.Parse(mockSheet["J3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 3), DateTime.Parse(mockSheet["K3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 4), DateTime.Parse(mockSheet["L3"].ToString()));
            Assert.AreEqual("testID", mockSheet["M3"]);
            Assert.AreEqual("5", mockSheet["N3"]);
        }

        [Test]
        public void Test_Sheet5Model_ApplyCattle_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "UK200968700548",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                listNumber = "LIST1"
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };

            int rowStart = 3;
            List<Cattle> cattleList = new List<Cattle>();
            cattleList.Add(cattle);

            // Act
            var mock = new Mock<Sheet5Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.ApplyCattle(rowStart, cattleList);


            // assert correct columns set
            Assert.AreEqual("UK200968700548", mockSheet["C3"]);
            Assert.AreEqual("UK200968", mockSheet["D3"]);
            Assert.AreEqual("7", mockSheet["E3"]);
            Assert.AreEqual("00548", mockSheet["F3"]);
            Assert.AreEqual("testBreed", mockSheet["G3"]);
            Assert.AreEqual("Female", mockSheet["H3"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["I3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 2), DateTime.Parse(mockSheet["J3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 3), DateTime.Parse(mockSheet["K3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 4), DateTime.Parse(mockSheet["L3"].ToString()));
            Assert.AreEqual("testID", mockSheet["M3"]);
            Assert.AreEqual("5", mockSheet["N3"]);
        }

        [Test]

        public void Test_CattleDetails_returns_CattleDetails()
        {
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "UK200968700548",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                listNumber = "LIST1"
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };

            CattleDetails cattleDetails = new CattleDetails(cattle, earTagBreakdown);

            //Assert

            Assert.AreEqual("UK200968700548", cattleDetails.earTag);
            Assert.AreEqual("1", cattleDetails.herdMark);
            Assert.AreEqual("2", cattleDetails.checkDigit);
            Assert.AreEqual("3", cattleDetails.animalNumber);
            Assert.AreEqual("testBreed", cattleDetails.breed);
            Assert.AreEqual("Female", cattleDetails.gender);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(cattleDetails.birthDate));
            Assert.AreEqual(new DateTime(2021, 3, 2), DateTime.Parse(cattleDetails.onDate));
            Assert.AreEqual(new DateTime(2021, 3, 3), DateTime.Parse(cattleDetails.offDate));
            Assert.AreEqual(new DateTime(2021, 3, 4), DateTime.Parse(cattleDetails.deathDate));
            Assert.AreEqual("testID", cattleDetails.damId);
            Assert.AreEqual("5", cattleDetails.passport);

        }

        [Test]

        public void Test_CattleDetails_returns_CattleDetails_BadData()
        {
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "UK200968700548",
                breedId = Guid.NewGuid(),
                Breed = null,
                gender = "Female",
                birthDate = new DateTime(1950, 01, 01),
                onDate = new DateTime(1950, 01, 01),
                offDate = new DateTime(1950, 01, 01),
                deathDate = new DateTime(1950, 01, 01),
                damId = "testID",
                passportVersion = "5",
                listNumber = "LIST1"
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };

            CattleDetails cattleDetails = new CattleDetails(cattle, earTagBreakdown);

            //Assert

            Assert.AreEqual("UK200968700548", cattleDetails.earTag);
            Assert.AreEqual("1", cattleDetails.herdMark);
            Assert.AreEqual("2", cattleDetails.checkDigit);
            Assert.AreEqual("3", cattleDetails.animalNumber);
            Assert.AreEqual("UNKNOWN", cattleDetails.breed);
            Assert.AreEqual("Female", cattleDetails.gender);
            Assert.AreEqual("11/11/1111", cattleDetails.birthDate);
            Assert.AreEqual("11/11/1111", cattleDetails.onDate);
            Assert.AreEqual("11/11/1111", cattleDetails.offDate);
            Assert.AreEqual("11/11/1111", cattleDetails.deathDate);
            Assert.AreEqual("testID", cattleDetails.damId);
            Assert.AreEqual("5", cattleDetails.passport);

        }

        [Test]
        public void Test_Sheet6Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "TestEarTag",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };


            int rowStart = 3;
            CattleDetailsCondensed cattleDetails = new CattleDetailsCondensed(cattle, earTagBreakdown);

            // Act
            var mock = new Mock<Sheet6Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.Apply(rowStart, cattleDetails);


            // assert correct columns set
            Assert.AreEqual("TestEarTag", mockSheet["A3"]);
            Assert.AreEqual("1", mockSheet["B3"]);
            Assert.AreEqual("2", mockSheet["C3"]);
            Assert.AreEqual("3", mockSheet["D3"]);
            Assert.AreEqual("testBreed", mockSheet["E3"]);
            Assert.AreEqual("Female", mockSheet["F3"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["G3"].ToString()));

        }

        [Test]
        public void Test_Sheet6Model_ApplyCattle_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "UK200968700548",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                listNumber = "LIST1"
            };


            int rowStart = 3;
            List<Cattle> cattleList = new List<Cattle>();
            cattleList.Add(cattle);

            // Act
            var mock = new Mock<Sheet6Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.ApplyCattle(rowStart, cattleList);


            // assert correct columns set
            Assert.AreEqual("UK200968700548", mockSheet["A3"]);
            Assert.AreEqual("UK200968", mockSheet["B3"]);
            Assert.AreEqual("7", mockSheet["C3"]);
            Assert.AreEqual("00548", mockSheet["D3"]);
            Assert.AreEqual("testBreed", mockSheet["E3"]);
            Assert.AreEqual("Female", mockSheet["F3"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["G3"].ToString()));
        }

        [Test]
        public void Test_Sheet7Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
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
                passportVersion = "5"
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };

            int rowStart = 3;
            string selection = "1st 10%";
            CattleDetails cattleDetails = new CattleDetails(cattle, earTagBreakdown);

            // Act
            var mock = new Mock<Sheet7Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.Apply(rowStart, cattleDetails, selection);


            // assert correct columns set
            Assert.AreEqual("TestEarTag", mockSheet["A3"]);
            Assert.AreEqual("1", mockSheet["B3"]);
            Assert.AreEqual("2", mockSheet["C3"]);
            Assert.AreEqual("3", mockSheet["D3"]);
            Assert.AreEqual("1st 10%", mockSheet["E3"]);
            Assert.AreEqual("testBreed", mockSheet["F3"]);
            Assert.AreEqual("Female", mockSheet["G3"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["H3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 2), DateTime.Parse(mockSheet["I3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 3), DateTime.Parse(mockSheet["J3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 4), DateTime.Parse(mockSheet["K3"].ToString()));
            Assert.AreEqual("testID", mockSheet["L3"]);
            Assert.AreEqual("5", mockSheet["M3"]);
        }

        [Test]
        public void Test_Sheet7Model_ApplyCattle_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
            Cattle cattle = new Cattle
            {
                earTag = "UK200968700548",
                breedId = Guid.NewGuid(),
                Breed = new Breed { Active = true, breedFull = "testBreed" },
                gender = "Female",
                birthDate = new DateTime(2021, 3, 1),
                onDate = new DateTime(2021, 3, 2),
                offDate = new DateTime(2021, 3, 3),
                deathDate = new DateTime(2021, 3, 4),
                damId = "testID",
                passportVersion = "5",
                listNumber = "LIST2"
            };
            List<string> earTagBreakdown = new List<string>
            {
                "1",
                "2",
                "3"
            };

            int rowStart = 3;
            List<Cattle> cattleList = new List<Cattle>();
            cattleList.Add(cattle);

            // Act
            var mock = new Mock<Sheet7Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.ApplyCattle(rowStart, cattleList, "1st 10");


            // assert correct columns set
            Assert.AreEqual("UK200968700548", mockSheet["A3"]);
            Assert.AreEqual("UK200968", mockSheet["B3"]);
            Assert.AreEqual("7", mockSheet["C3"]);
            Assert.AreEqual("00548", mockSheet["D3"]);
            Assert.AreEqual("1st 10", mockSheet["E3"]);
            Assert.AreEqual("testBreed", mockSheet["F3"]);
            Assert.AreEqual("Female", mockSheet["G3"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["H3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 2), DateTime.Parse(mockSheet["I3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 3), DateTime.Parse(mockSheet["J3"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 4), DateTime.Parse(mockSheet["K3"].ToString()));
            Assert.AreEqual("testID", mockSheet["L3"]);
            Assert.AreEqual("5", mockSheet["M3"]);
        }

        [Test]
        public void Test_Sheet9Model_Apply_UpdatesCells()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
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

            Cattle cattle1 = new Cattle
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
                notificationDate = new DateTime(2021, 5, 14),
                lateType = "LATE_OFF"
            };

            Cattle cattle2 = new Cattle
            {
                earTag = "TestEarTag1",
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


            List<Cattle> cattleList = new List<Cattle>();

            cattleList.Add(cattle);
            cattleList.Add(cattle1);
            cattleList.Add(cattle2);

            int rowStart = 30;

            // Act
            var mock = new Mock<Sheet9Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            holidayService.Setup(x => x.GetWorkingDays(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(10);

            var model = mock.Object;
            model.ApplyCattle(rowStart, cattleList, new DateTime(2021, 3, 4), holidayService.Object);


            // assert correct columns set
            Assert.AreEqual("TestEarTag", mockSheet["B30"]);
            Assert.AreEqual("testBreed", mockSheet["C30"]);
            Assert.AreEqual("Female", mockSheet["D30"]);
            Assert.AreEqual(new DateTime(2021, 3, 1), DateTime.Parse(mockSheet["E30"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 2), DateTime.Parse(mockSheet["F30"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 3), DateTime.Parse(mockSheet["G30"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 4), DateTime.Parse(mockSheet["H30"].ToString()));
            Assert.AreEqual("testID", mockSheet["I30"]);
            Assert.AreEqual("5", mockSheet["J30"]);
            Assert.AreEqual("FMXC 1", mockSheet["K30"]);
            Assert.AreEqual("On", mockSheet["L30"]);
            Assert.AreEqual(new DateTime(2021, 3, 6), DateTime.Parse(mockSheet["M30"].ToString()));
            Assert.AreEqual(new DateTime(2021, 3, 21), DateTime.Parse(mockSheet["N30"].ToString()));
            Assert.AreEqual("FMXC 1", mockSheet["K31"]);
            Assert.AreEqual("Off", mockSheet["L31"]);

        }

        [Test]
        public void Test_Sheet9Model_Calculate_Cattle_Stats()
        {
            var mockSheet = new Dictionary<string, Object>();
            // Arrange
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

            Cattle cattle1 = new Cattle
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
                notificationDate = new DateTime(2021, 5, 14),
                lateType = "LATE_OFF"
            };

            Cattle cattle2 = new Cattle
            {
                earTag = "TestEarTag1",
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


            List<Cattle> cattleList = new List<Cattle>();

            cattleList.Add(cattle);
            cattleList.Add(cattle1);
            cattleList.Add(cattle2);

            int totalMovesThisYear = 30;

            holidayService.Setup(x => x.GetWorkingDays(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(10);

            // Act
            var mock = new Mock<Sheet9Model>(null);
            mock.Setup(x => x.SetCell(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string cell, object value) => mockSheet.Add(cell, value));

            var model = mock.Object;
            model.CalculateCattleStats(cattleList, totalMovesThisYear, holidayService.Object);


            // assert correct columns set
            Assert.AreEqual(30, mockSheet["I13"]);
            Assert.AreEqual(3, mockSheet["C19"]);
            Assert.AreEqual(0, mockSheet["C20"]);
            Assert.AreEqual(0, mockSheet["C21"]);
            Assert.AreEqual(0, mockSheet["C22"]);
            Assert.AreEqual(0, mockSheet["C23"]);
            Assert.AreEqual(0, mockSheet["C24"]);
            Assert.AreEqual(0, mockSheet["C25"]);
            Assert.AreEqual(0, mockSheet["C26"]);
            Assert.AreEqual("Yes", mockSheet["I21"]);

        }


    }
}