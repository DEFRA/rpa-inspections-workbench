using OfficeOpenXml;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using RPA.Inspections.Workbench.Services.Functions;
using RPA.Inspections.Workbench.Services.SheetModels;
using RPA.Inspections.Workbench.Services.SheetModels.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    [System.Runtime.InteropServices.Guid("4FF61BCB-8A75-49AD-B819-CCE7DCDF940E")]
    public class FileService : IFileService
    {

        IWorkbenchContext db;

        ICattleService cattleService;
        IHoldingService holdingService;
        IMailService mailService;
        ISendService sendService;
        IHolidayService holidayService;


        public FileService(IWorkbenchContext context, ICattleService cattleService, IHoldingService holdingService, ISendService sendService, IMailService mailService, IHolidayService holidayService)
        {
            this.db = context;
            this.cattleService = cattleService;
            this.holdingService = holdingService;
            this.sendService = sendService;
            this.mailService = mailService;
            this.holidayService = holidayService;
        }

        //Run all Processes to Add new Inspection List

        public void RunAddList(HttpPostedFileBase bulkSource, bool deleteAll)
        {
            if (deleteAll == true)
            {
                ClearTables();
            }
            DataTable table = BulkData(bulkSource);

            foreach (DataRow row in table.Rows)
            {
                int i = 2;

                try
                {
                    AddToHolding(row);

                    i++;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error at row {0}", i), ex);
                }
            }

        }


        //Update Holding Table with new Inspection List File

        public void AddToHolding(DataRow row)
        {
            string blankTest = row[0].ToString();

            if (blankTest != "")
            {
                Holding NewHolding = new Holding();


                NewHolding.cphNumber = row[0].ToString();

                int sbi;

                if (int.TryParse(row[1].ToString(), out sbi))
                {
                    sbi = int.Parse(row[1].ToString());
                    NewHolding.SBI = sbi;
                }


                string schyr = row[2].ToString();
                string schemeName = row[3].ToString();

                if (!string.IsNullOrEmpty(schyr))
                {
                    NewHolding.schemeYear = string.Format("20{0}", schyr.Substring((schyr.Length - 2), 2));
                }
                else
                {
                    NewHolding.schemeYear = schyr;
                }

                if (!string.IsNullOrEmpty(schemeName))
                {

                    if (schemeName.Substring(0, 3) == "CII")
                    {
                        string SchemeDropString = schemeName + schyr.Substring(schyr.Length - 2);
                        NewHolding.SchemeName = SchemeDropString;

                        SchemeNameDrop schemeNameDrop = db.SchemeNameDrop.Where(x => x.Text == SchemeDropString).FirstOrDefault();
                        NewHolding.SchemeNameDropId = schemeNameDrop.SchemeNameDropId;
                    }


                    else
                    {
                        NewHolding.SchemeName = schemeName;
                    }

                }


                string selMed = row[4].ToString().ToUpper();

                if (selMed == "RISK" || selMed == "RANDOM" || selMed == "TARGETED" || selMed == "AUDIT")
                {
                    NewHolding.selectionMethod = selMed;
                }
                else
                {
                    NewHolding.selectionMethod = "Other";
                }

                string Inspector = row[5].ToString().ToUpper();

                if (!String.IsNullOrEmpty(Inspector))
                {
                    NewHolding.AssignedUser = Inspector;
                }

                //no longer in 2017 Inspection List Worksheet LG
                //now in 2019 Inspection List Worksheet SD

                int aniInsp = 0;

                if (int.TryParse(row[6].ToString(), out aniInsp))
                {
                    aniInsp = int.Parse(row[6].ToString());
                    NewHolding.animalsToBeInspected = aniInsp;
                }

                DateTime inspDate = DateTime.Now;

                if (DateTime.TryParse(row[7].ToString(), out inspDate))
                {
                    NewHolding.inspectionDeadlineDate = inspDate;
                }

                db.Holding.Add(NewHolding);
            }
            db.SaveChanges();
        }

        //Accept a New Inspection List File

        public DataTable BulkData(HttpPostedFileBase bulkSource)
        {
            string filepath = PathBuilder(Path.GetFileName(bulkSource.FileName));
            bulkSource.SaveAs(filepath);

            DataTable BulkSource = new DataTable("BulkSource");

            bool firstRowIsHeader = true;

            FileInfo existingFile = new FileInfo(filepath);

            ExcelPackage ePack = new ExcelPackage(existingFile);
            ExcelWorksheet ws = ePack.Workbook.Worksheets.First();

            for (int xx = 1; xx <= ws.Dimension.End.Column; xx++)
            {
                string data = "";
                if (ws.Cells[1, xx].Value != null)
                {
                    data = ws.Cells[1, xx].Value.ToString();

                    string columnName = firstRowIsHeader ? data : "column" + xx.ToString();
                    BulkSource.Columns.Add(columnName);
                }
            }

            int first = firstRowIsHeader ? 2 : 1;


            int lastRow = GetLastUsedRow(ws);

            for (int excelRow = first; excelRow <= lastRow; excelRow++)
            {
                DataRow rw = BulkSource.NewRow();
                BulkSource.Rows.Add(rw);

                for (int excelCol = 1; excelCol <= ws.Dimension.End.Column; excelCol++)
                {
                    string data = "";
                    if (ws.Cells[excelRow, excelCol].Value != null)
                    {
                        data = ws.Cells[excelRow, excelCol].Value.ToString();
                    }
                    rw[excelCol - 1] = data;
                }
            }

            File.Delete(filepath);

            return BulkSource;
        }

        //get last row of Excel Spreadsheet

        private int GetLastUsedRow(ExcelWorksheet sheet)
        {
            var row = sheet.Dimension.End.Row;
            while (row >= 1)
            {
                var range = sheet.Cells[row, 1, row, sheet.Dimension.End.Column];
                if (range.Any(c => !string.IsNullOrEmpty(c.Text)))
                {
                    break;
                }
                row--;
            }
            return row;
        }

        //Delete existing data in Cattle, Holding, LinkedHolding and PackRequested Tables

        public void ClearTables()
        {
            List<Control> ActiveSchemeYrs = db.Control.Where(x => x.Active == true && x.Property == "SchemeYr").ToList();

            foreach (var item in db.PackRequested.ToList())
            {
                db.PackRequested.Remove(item);
            }

            foreach (var item in db.Cattle.ToList())
            {
                db.Cattle.Remove(item);
            }

            foreach (var item in db.LinkedHolding.ToList())
            {
                db.LinkedHolding.Remove(item);
            }

            foreach (var schemeYr in ActiveSchemeYrs)
            {
                foreach (var item in db.Holding.Where(x => x.schemeYear == schemeYr.Value).ToList())
                {
                    db.Holding.Remove(item);
                }
            }

            db.SaveChanges();
        }

        // Read File from Location

        public byte[] StreamFile(string savepath)
        {
            FileStream fs = new FileStream(savepath, FileMode.Open, FileAccess.Read);

            byte[] packFile = new byte[fs.Length];

            fs.Read(packFile, 0, System.Convert.ToInt32(fs.Length));

            fs.Close();
            return packFile;
        }


        //Run 1, 2, 3 below. 
        public void RunAllBelow(Guid HoldingId)
        {
            Holding model = holdingService.GetHoldingById(HoldingId);


            PackRequested prmodel = db.PackRequested.Where(x => x.holdingId == HoldingId && x.activeRequest == true).FirstOrDefault();


            string savepath = InspectionPackCreate(model, prmodel);
            UpdatePackReq(model, prmodel, savepath);
            MailMessage message = mailService.GenerateInspectorEmail(model, prmodel);

            sendService.sendEmail(message);
        }



        //1. Remove Pack Requested Flag on File Return, Update Last Data Refresh, Make Pack Available For Download


        public void UpdatePackReq(Holding model, PackRequested prmodel, string savepath)
        {

            prmodel.activeRequest = false;
            model.lastDataRefresh = DateTime.Now;

            prmodel.savePath = savepath;
            prmodel.packGenerated = DateTime.Now;

            db.SetModified(model);
            db.SetModified(prmodel);

            db.SaveChanges();

        }


        //3. Populate Animal Inspection Pack

        public string InspectionPackCreate(Holding model, PackRequested prmodel)
        {

            string safeCPH = SafeCPHBuilder(model.cphNumber);

            Guid holdingId = model.holdingId;

            string filepath = PathBuilder("CII Animal Data Spreadsheet v2.2.xlsm");
            string savepath = PathBuilder("CII Animal Data Spreadsheet" + safeCPH + ".xlsm");

            FileInfo existingFile = new FileInfo(filepath);

            ExcelPackage ePack = new ExcelPackage(existingFile);

            ExcelWorksheet worksheet1 = ePack.Workbook.Worksheets["Inspection Data"];
            ExcelWorksheet worksheet2 = ePack.Workbook.Worksheets["Keeper Contact Details"];
            ExcelWorksheet worksheet3 = ePack.Workbook.Worksheets["CTS Links"];
            ExcelWorksheet worksheet4 = ePack.Workbook.Worksheets["Summary"];
            ExcelWorksheet worksheet5 = ePack.Workbook.Worksheets["Animal List 1"];
            ExcelWorksheet worksheet6 = ePack.Workbook.Worksheets["Animal List 1 (Condensed)"];
            ExcelWorksheet worksheet7 = ePack.Workbook.Worksheets["Animal List 2"];
            ExcelWorksheet worksheet8 = ePack.Workbook.Worksheets["Additional Animal List"];
            ExcelWorksheet worksheet9 = ePack.Workbook.Worksheets["Historic and Assessment"];

            string actualSchemeYear = model.SchemeName != null ? "20" + model.SchemeName.Substring(model.SchemeName.Length - 2) : model.schemeYear;

            var sheetModel = new Sheet1Model(DateTime.Now, model, prmodel, worksheet1.Cells);
            sheetModel.Apply();

            var sheetModel2 = new Sheet2Model(model, worksheet2.Cells);
            sheetModel2.Apply();

            //get all linked holdings and add them to spreadsheet here

            List<LinkedHolding> linkedHoldingList = db.LinkedHolding.Where(x => x.holdingId == holdingId).ToList();

            //update CTS Links Page
            int rowStart = 3;

            var sheetModel3 = new Sheet3Model(worksheet3.Cells);
            sheetModel3.ApplyLinkedHoldings(rowStart, linkedHoldingList);


            //get all animals and add to spreadsheet here

            //get a list of all cattle for this holding

            List<Cattle> cattleList = cattleService.GetCattleByHoldingId(holdingId);


            //udpate the Summary Worksheet - now done in Spreadsheet

            var sheetModel4 = new Sheet4Model(cattleList, worksheet4.Cells);
            sheetModel4.Apply();

            int rowStartCattleList1 = 3;

            //Animal List 1
            var sheetModel5 = new Sheet5Model(worksheet5.Cells);
            sheetModel5.ApplyCattle(rowStartCattleList1, cattleList);

            //Animal List 1 Condensed
            var sheetModel6 = new Sheet6Model(worksheet6.Cells);
            sheetModel6.ApplyCattle(rowStartCattleList1, cattleList);



            if (sheetModel4.listTwoCount != 0)
            {
                {
                    // calculate how many cows make up 10% of list 2


                    List<Cattle> cattleSelectionOne = TenPercentCattleSelector.CattleSelector(1, cattleList, sheetModel4.listTwoCount);
                    List<Cattle> cattleSelectionTwo = TenPercentCattleSelector.CattleSelector(2, cattleList, sheetModel4.listTwoCount); 
                    List<Cattle> cattleSelectionThree = TenPercentCattleSelector.CattleSelector(3, cattleList, sheetModel4.listTwoCount);

                    int rowStartCattleList2 = 3;

                    var sheetModel7 = new Sheet7Model(worksheet7.Cells);
                    rowStartCattleList2 = sheetModel7.ApplyCattle(rowStartCattleList2, cattleSelectionOne, "1st 10%");
                    rowStartCattleList2 = sheetModel7.ApplyCattle(rowStartCattleList2, cattleSelectionTwo, "2nd 10%");
                    sheetModel7.ApplyCattle(rowStartCattleList2, cattleSelectionThree, "");

                }

            }

            //populate the historic and assessment page

            List<Cattle> LateCattleList = cattleService.GetLateCattle(holdingId);

            var sheetModel9 = new Sheet9Model(worksheet9.Cells);
            int rowStartCattleListLate = 30;

            sheetModel9.ApplyCattle(rowStartCattleListLate, LateCattleList, DateTime.Now, holidayService);
            sheetModel9.CalculateCattleStats(LateCattleList, model.TotalMovesThisYear, holidayService);


            FileInfo saving = new FileInfo(savepath);

            ePack.SaveAs(saving);

            return (savepath);

        }

        public string PathBuilder(string file)
        {

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", file);

            return path;
        }

        public string SafeCPHBuilder(string cph)
        {
            return Regex.Replace(cph, "[^0-9a-zA-Z]+", "");
        }




       

    }
}

