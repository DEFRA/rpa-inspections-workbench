using RPA.Inspections.Workbench.Tests.Data.Mock;
using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.Services.Functions;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using RPA.Inspections.Workbench.Tests.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO.Compression;
using System.Security.Cryptography;

namespace RPA.Inspections.Workbench.Tests.SL
{
    [TestFixture]
    [Category("File Service")]
    public class FileServiceTests
    {
        MockWorkbenchContext context;
        MockPeopleContext peopleContext;

        IFileService fileService;
        ICattleService cattleService;
        Mock<IMailService> mailService;
        IHoldingService holdingService;
        IFilterService filterService;
        IUserHelper userHelper;
        Mock<ISendService> sendService;
        Mock<IHolidayService> holidayService;
        Mock<IFileStorage> fileStorage;


        [SetUp]
        public void Setup()
        {
            context = new MockWorkbenchContext();
            peopleContext = new MockPeopleContext();
            cattleService = new CattleService(context.Context);
            sendService = new Mock<ISendService>();
            holidayService = new Mock<IHolidayService>();
            mailService = new Mock<IMailService>();
            filterService = new FilterService(context.Context, peopleContext.Context);
            userHelper = new UserHelper(peopleContext.Context, MockHttpContext.GetMockedHttpContextAccessor());
            holdingService = new HoldingService(context.Context, peopleContext.Context, filterService, userHelper);
            fileStorage = new Mock<IFileStorage>();
            fileService = new FileService(context.Context, cattleService, holdingService, sendService.Object, mailService.Object, holidayService.Object, fileStorage.Object);
        }

        [Test]

        public void Test_AddToHolding_AddsHolding()
        {
            //Arrange

            DataTable fakeHolding = new DataTable();

            for (int i = 0; i < 9; i++)
            {
                fakeHolding.Columns.Add();
            }

            DataRow newrow = fakeHolding.NewRow();

            newrow[0] = "01/011/0111";
            newrow[1] = "123456789";
            newrow[2] = "2020";
            newrow[3] = "CII";
            newrow[4] = "RISK";
            newrow[5] = "M600300";
            newrow[6] = "50";
            newrow[7] = DateTime.Now;


            //Act

            fileService.AddToHolding(newrow);

            //Assert

            Assert.AreEqual(7, context.Context.Holding.Count());
            Assert.AreEqual(7, context.NewContext().Holding.Count());
        }

        [Test]

        public void Test_AddToHolding_AddsHolding_handles_empty_values()
        {
            //Arrange

            DataTable fakeHolding = new DataTable();

            for (int i = 0; i < 9; i++)
            {
                fakeHolding.Columns.Add();
            }

            DataRow newrow = fakeHolding.NewRow();

            newrow[0] = "01/011/0111";
            newrow[1] = "123456789";
            newrow[2] = "20";
            newrow[3] = "EGG";
            newrow[4] = "";
            newrow[5] = "M600300";
            newrow[6] = "50";
            newrow[7] = DateTime.Now;


            //Act

            fileService.AddToHolding(newrow);

            //Assert

            Assert.AreEqual(7, context.Context.Holding.Count());
            Assert.AreEqual(7, context.NewContext().Holding.Count());
        }

        [Test]

        public void Test_UpdatePackReq_UpdatesPackReq()
        {
            //Arrange


            Holding holding = context.Context.Holding.Where(x => x.holdingId == Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF70")).First();
            PackRequested packRequested = context.Context.PackRequested.Where(x => x.packRequestedId == Guid.Parse("A4609495-1FA5-4BEB-9B09-00777D219F31")).First();


            //Act

            fileService.UpdatePackReq(holding, packRequested, "TestPath");

            //Assert

            Assert.AreEqual("TestPath", context.NewContext().PackRequested.Single(x => x.packRequestedId == packRequested.packRequestedId).savePath);
        }



        [Test]
        public void TestClearTables_ClearsTables()
        {

            //Arrange


            //Act

            fileService.ClearTables();

            //Assert

            Assert.AreEqual(0, context.Context.PackRequested.Count());
            Assert.AreEqual(0, context.Context.Cattle.Count());
            Assert.AreEqual(0, context.Context.LinkedHolding.Count());
            Assert.AreEqual(2, context.Context.Holding.Count());
        }


       [Test]

        public void Test_safe_CPH_builder_returns_safe_CPH()
        {

            //Arrange
            string dodgyCPH = "30  /123/  1234";

            //Act

            var result = fileService.SafeCPHBuilder(dodgyCPH);

            //Assert

            Assert.AreEqual("301231234", result);


        }

        [Test]

        public void Test_PathBuilder_Returns_Path()
        {
            //Arrange

            //Act

            var result = fileService.PathBuilder("Test");

            //Assert


            Assert.AreEqual(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", "Test"), result);

        }

        [Test]
        public void BulkImportReadsWorkbookInMemoryWithoutCreatingFiles()
        {
            var before = Directory.GetFiles(fileService.PathBuilder("")).OrderBy(path => path).ToArray();
            using var source = new MemoryStream();
            using (var workbook = new ExcelPackage())
            {
                var sheet = workbook.Workbook.Worksheets.Add("Import");
                sheet.Cells[1, 1].Value = "CPH";
                sheet.Cells[1, 2].Value = "SBI";
                sheet.Cells[2, 1].Value = "12/345/6789";
                sheet.Cells[2, 2].Value = "123456789";
                workbook.SaveAs(source);
            }
            source.Position = 0;
            var upload = new FormFile(source, 0, source.Length, "bulkSource", "../../untrusted-name.xlsx");

            var table = fileService.BulkData(upload);

            Assert.That(table.Rows, Has.Count.EqualTo(1));
            Assert.That(table.Rows[0][0], Is.EqualTo("12/345/6789"));
            Assert.That(Directory.GetFiles(fileService.PathBuilder("")).OrderBy(path => path), Is.EqualTo(before));
            fileStorage.VerifyNoOtherCalls();
        }

        [Test]
        public void GeneratedPacksUploadMacroWorkbooksWithUniqueKeysWithoutWritingLocalFiles()
        {
            var before = Directory.GetFiles(fileService.PathBuilder("")).OrderBy(path => path).ToArray();
            var template = fileService.PathBuilder("CII Animal Data Spreadsheet v2.2.xlsm");
            var templateHash = SHA256.HashData(File.ReadAllBytes(template));
            var holding = new Holding { cphNumber = "12/345/6789", SchemeName = "CII26", primaryName = "Blob test holding", selectionMethod = "RANDOM", schemeYear = "2026" };
            var request = new PackRequested(holding.holdingId);
            var uploadedNames = new List<string>();
            fileStorage.Setup(value => value.Upload(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .Callback((string name, Stream content, string contentType) =>
                {
                    uploadedNames.Add(name);
                    Assert.That(contentType, Is.EqualTo("application/vnd.ms-excel.sheet.macroEnabled.12"));
                    using var archive = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true);
                    Assert.That(archive.GetEntry("xl/workbook.xml"), Is.Not.Null);
                    Assert.That(archive.GetEntry("xl/vbaProject.bin"), Is.Not.Null);
                });

            var first = fileService.InspectionPackCreate(holding, request);
            var second = fileService.InspectionPackCreate(holding, request);

            Assert.That(first, Does.StartWith($"packs/{request.packRequestedId:N}/"));
            Assert.That(first, Does.EndWith("/CII Animal Data Spreadsheet123456789.xlsm"));
            Assert.That(second, Is.Not.EqualTo(first));
            Assert.That(uploadedNames, Is.EqualTo(new[] { first, second }));
            Assert.That(SHA256.HashData(File.ReadAllBytes(template)), Is.EqualTo(templateHash));
            Assert.That(Directory.GetFiles(fileService.PathBuilder("")).OrderBy(path => path), Is.EqualTo(before));
        }

        [Test]
        public void FailedPackUploadDoesNotMarkRequestGeneratedOrSendNotification()
        {
            var holding = new Holding { cphNumber = "12/345/6789", SchemeName = "CII26", primaryName = "Blob test holding", selectionMethod = "RANDOM", schemeYear = "2026" };
            var request = new PackRequested(holding.holdingId);
            context.Context.Holding.Add(holding);
            context.Context.PackRequested.Add(request);
            context.Context.SaveChanges();
            var holdingLookup = new Mock<IHoldingService>();
            holdingLookup.Setup(value => value.GetHoldingById(holding.holdingId)).Returns(holding);
            var generator = new FileService(context.Context, cattleService, holdingLookup.Object,
                sendService.Object, mailService.Object, holidayService.Object, fileStorage.Object);
            fileStorage.Setup(value => value.Upload(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Offline storage failure."));

            Assert.Throws<InvalidOperationException>(() => generator.RunAllBelow(holding.holdingId));
            Assert.That(request.activeRequest, Is.True);
            Assert.That(request.packGenerated, Is.Null);
            Assert.That(request.savePath, Is.Null);
            Assert.That(context.NewContext().PackRequested.Count(), Is.EqualTo(PackRequestedData.Data().Count + 1));
            sendService.VerifyNoOtherCalls();
        }

        [Test]
        public void CompletedPackIsRecordedOnlyAfterUpload()
        {
            var holding = new Holding { cphNumber = "12/345/6789", SchemeName = "CII26", primaryName = "Blob test holding", selectionMethod = "RANDOM", schemeYear = "2026" };
            var request = new PackRequested(holding.holdingId);
            context.Context.Holding.Add(holding);
            context.Context.PackRequested.Add(request);
            context.Context.SaveChanges();
            var holdingLookup = new Mock<IHoldingService>();
            holdingLookup.Setup(value => value.GetHoldingById(holding.holdingId)).Returns(holding);
            var generator = new FileService(context.Context, cattleService, holdingLookup.Object,
                sendService.Object, mailService.Object, holidayService.Object, fileStorage.Object);
            var events = new List<string>();
            string uploadedName = null;
            fileStorage.Setup(value => value.Upload(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .Callback((string name, Stream content, string contentType) =>
                {
                    uploadedName = name;
                    events.Add("uploaded");
                });
            context.Context.SavingChanges += (_, _) => events.Add("saved");

            generator.RunAllBelow(holding.holdingId);

            Assert.That(events, Is.EqualTo(new[] { "uploaded", "saved" }));
            Assert.That(request.savePath, Is.EqualTo(uploadedName));
            Assert.That(request.packGenerated, Is.Not.Null);
            Assert.That(request.activeRequest, Is.False);
            sendService.Verify(value => value.sendEmail(It.IsAny<System.Net.Mail.MailMessage>()), Times.Once);
        }

        [Test]

        public void Test_TenPercentCattleSelector_Selects_Correctly_Option_1()
        {
            //Arrange

            List<Cattle> cattleList = context.Context.Cattle.ToList();

            //Act

            var result = TenPercentCattleSelector.CattleSelector(1, cattleList, 10);

            //Assert

            Assert.AreEqual(1, result.Count);

        }

        [Test]

        public void Test_TenPercentCattleSelector_Selects_Correctly_Option_2()
        {
            //Arrange

            List<Cattle> cattleList = context.Context.Cattle.ToList();

            //Act

            var result = TenPercentCattleSelector.CattleSelector(2, cattleList, 10);

            //Assert

            Assert.AreEqual(1, result.Count);

        }

        [Test]

        public void Test_TenPercentCattleSelector_Selects_Correctly_Option_3()
        {
            //Arrange

            List<Cattle> cattleList = context.Context.Cattle.ToList();

            //Act

            var result = TenPercentCattleSelector.CattleSelector(3, cattleList, 10);

            //Assert

            Assert.AreEqual(2, result.Count);

        }



    }
}






