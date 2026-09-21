using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet1Model : SheetModelBase
    {
        public string cphNumber { get; set; }
        public int? SBI { get; set; }
        public string primaryName { get; set; }
        public string currentTime { get; set; }
        public string showRef { get; set; }
        public string dateRequested { get; set; }
        public string requestedBy { get; set; }

        public Sheet1Model(DateTime timeNow, Holding holding, PackRequested packRequested, ExcelRange cells) : base(cells)
        {
            cphNumber = holding.cphNumber;
            SBI = holding.SBI;
            primaryName = holding.primaryName;
            currentTime = timeNow.ToString("dd-MM-yyyy h:mm tt");
            dateRequested = packRequested.datePackRequested.ToString("dd-MM-yyyy h:mm tt");
            showRef = packRequested.showRef;
            requestedBy = packRequested.requestedByName;
        }

        public void Apply()
        {
            SetCell("E10", cphNumber);
            SetCell("E12", SBI);
            SetCell("D14", primaryName);
            SetCell("H1", currentTime);
            SetCell("H2", showRef);
            SetCell("H3", dateRequested);
            SetCell("D37", requestedBy);
        }

    }
}