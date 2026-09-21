using OfficeOpenXml;
using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Services.SheetModels
{
    public class Sheet2Model : SheetModelBase
    {
        public string primaryName { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string region { get; set; }
        public string postCode { get; set; }
        public string telephoneNumber { get; set; }
        public string faxNumber { get; set; }
        public string mobileNumber { get; set; }
        public int? SBI { get; set; }
        public string schemeYear { get; set; }
        public string selectionMethod { get; set; }

        public Sheet2Model(Holding holding, ExcelRange cells) : base(cells)
        {
            primaryName = holding.primaryName;
            addressLine1 = holding.addressLine1;
            addressLine2 = holding.addressLine2;
            addressLine3 = holding.addressLine3;
            region = holding.region;
            postCode = holding.postCode;
            telephoneNumber = holding.telephoneNumber;
            faxNumber = holding.faxNumber;
            mobileNumber = holding.mobileNumber;
            SBI = holding.SBI;
            schemeYear = holding.SchemeName != null ? "20" + holding.SchemeName.Substring(holding.SchemeName.Length - 2) : holding.schemeYear;
            selectionMethod = holding.selectionMethod;
        }

        public void Apply()
        {
            SetCell("B2", primaryName);
            SetCell("B3", addressLine1);
            SetCell("B4", addressLine2);
            SetCell("B5", addressLine3);
            SetCell("B6", region);
            SetCell("B8", postCode);
            SetCell("B11", telephoneNumber);
            SetCell("B12", faxNumber);
            SetCell("B13", mobileNumber);
            SetCell("B14", SBI);
            SetCell("B17", schemeYear);
            SetCell("B18", selectionMethod);
        }

    }
}